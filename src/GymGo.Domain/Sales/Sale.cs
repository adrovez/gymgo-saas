using GymGo.Domain.Cash;
using GymGo.Domain.Common;
using GymGo.Domain.Exceptions;

namespace GymGo.Domain.Sales;

/// <summary>
/// Cabecera de venta de servicios.
/// Agrupa ítems cobrables (plan, matrícula, servicios extra) y descuentos.
/// Al confirmarse, genera automáticamente la MembershipAssignment y la CashTransaction.
///
/// Invariantes:
/// - MemberId y TenantId son inmutables.
/// - Solo se pueden agregar/modificar ítems y descuentos en estado Draft.
/// - Solo puede existir un ítem de tipo Plan por venta.
/// - TotalAmount = SubtotalAmount - DiscountAmount >= 0.
/// - Confirm() requiere al menos un ítem.
/// - Solo una venta Confirmed puede anularse; la anulación es irreversible.
/// </summary>
public sealed class Sale : AggregateRoot, IAuditable, ITenantScoped, ISoftDeletable
{
    // ── Tenant ────────────────────────────────────────────────────────────
    public Guid TenantId { get; set; }

    // ── Socio ─────────────────────────────────────────────────────────────
    public Guid MemberId { get; private set; }

    // ── Clasificación ─────────────────────────────────────────────────────
    public DateOnly Date { get; private set; }
    public SaleStatus Status { get; private set; }
    public CashPaymentMethod PaymentMethod { get; private set; }

    // ── Montos ────────────────────────────────────────────────────────────
    public decimal SubtotalAmount { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TotalAmount    { get; private set; }

    // ── Vínculos generados al confirmar ──────────────────────────────────
    public Guid? MembershipAssignmentId { get; private set; }
    public Guid? CashTransactionId { get; private set; }

    // ── Observaciones ─────────────────────────────────────────────────────
    public string? Notes { get; private set; }

    // ── Auditoría de quien procesó la venta ──────────────────────────────
    public Guid ProcessedByUserId { get; private set; }

    // ── Anulación ─────────────────────────────────────────────────────────
    public DateTime? VoidedAtUtc { get; private set; }
    public string? VoidReason { get; private set; }

    // ── IAuditable ────────────────────────────────────────────────────────
    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedAtUtc { get; set; }
    public string? ModifiedBy { get; set; }

    // ── ISoftDeletable ────────────────────────────────────────────────────
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public string? DeletedBy { get; set; }

    // ── Navegación ────────────────────────────────────────────────────────
    private readonly List<SaleItem> _items = new();
    private readonly List<SaleDiscount> _discounts = new();

    public IReadOnlyCollection<SaleItem> Items => _items.AsReadOnly();
    public IReadOnlyCollection<SaleDiscount> Discounts => _discounts.AsReadOnly();

    // ── EF Core ───────────────────────────────────────────────────────────
    private Sale() { }

    private Sale(
        Guid id, Guid tenantId, Guid memberId,
        DateOnly date, CashPaymentMethod paymentMethod,
        string? notes, Guid processedByUserId)
        : base(id)
    {
        TenantId          = tenantId;
        MemberId          = memberId;
        Date              = date;
        Status            = SaleStatus.Draft;
        PaymentMethod     = paymentMethod;
        SubtotalAmount    = 0;
        DiscountAmount    = 0;
        TotalAmount       = 0;
        Notes             = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        ProcessedByUserId = processedByUserId;
    }

    // ── Factory ───────────────────────────────────────────────────────────

    public static Sale Create(
        Guid tenantId,
        Guid memberId,
        DateOnly date,
        CashPaymentMethod paymentMethod,
        string? notes,
        Guid processedByUserId)
    {
        if (tenantId == Guid.Empty)
            throw new BusinessRuleViolationException("SALE_TENANT_REQUIRED", "El tenant es obligatorio.");

        if (memberId == Guid.Empty)
            throw new BusinessRuleViolationException("SALE_MEMBER_REQUIRED", "El socio es obligatorio.");

        if (processedByUserId == Guid.Empty)
            throw new BusinessRuleViolationException("SALE_USER_REQUIRED", "El usuario que procesa la venta es obligatorio.");

        if (notes?.Length > 500)
            throw new BusinessRuleViolationException("SALE_NOTES_TOO_LONG", "Las observaciones no pueden superar los 500 caracteres.");

        return new Sale(Guid.NewGuid(), tenantId, memberId, date, paymentMethod, notes, processedByUserId);
    }

    // ── Gestión de ítems ──────────────────────────────────────────────────

    public SaleItem AddItem(
        SaleItemType itemType,
        string description,
        Guid? membershipPlanId,
        decimal unitPrice,
        int quantity = 1)
    {
        EnsureDraft();

        if (itemType == SaleItemType.Plan && _items.Any(i => i.ItemType == SaleItemType.Plan))
            throw new BusinessRuleViolationException(
                "SALE_PLAN_DUPLICATE", "Solo puede existir un ítem de tipo Plan por venta.");

        var item = SaleItem.Create(TenantId, Id, itemType, description, membershipPlanId, unitPrice, quantity);
        _items.Add(item);
        RecalculateTotals();
        return item;
    }

    public void UpdateItem(Guid itemId, string description, decimal unitPrice, int quantity)
    {
        EnsureDraft();

        var item = _items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new NotFoundException("SaleItem", itemId);

        item.Update(description, unitPrice, quantity);
        RecalculateTotals();
    }

    public void RemoveItem(Guid itemId)
    {
        EnsureDraft();

        var item = _items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new NotFoundException("SaleItem", itemId);

        _items.Remove(item);
        RecalculateTotals();
    }

    // ── Gestión de descuentos ─────────────────────────────────────────────

    public SaleDiscount AddDiscount(SaleDiscountType discountType, string description, decimal value)
    {
        EnsureDraft();

        var discount = SaleDiscount.Create(TenantId, Id, discountType, description, value, SubtotalAmount);

        // Verificar que la suma de descuentos no supere el subtotal
        var totalDiscount = _discounts.Sum(d => d.Amount) + discount.Amount;
        if (totalDiscount > SubtotalAmount)
            throw new BusinessRuleViolationException(
                "SALE_DISCOUNT_EXCEEDS_SUBTOTAL",
                $"El descuento total ({totalDiscount:N2}) supera el subtotal de la venta ({SubtotalAmount:N2}).");

        _discounts.Add(discount);
        RecalculateTotals();
        return discount;
    }

    public void RemoveDiscount(Guid discountId)
    {
        EnsureDraft();

        var discount = _discounts.FirstOrDefault(d => d.Id == discountId)
            ?? throw new NotFoundException("SaleDiscount", discountId);

        _discounts.Remove(discount);
        RecalculateTotals();
    }

    // ── Confirmación ──────────────────────────────────────────────────────

    /// <summary>
    /// Confirma la venta. Los vínculos (MembershipAssignmentId, CashTransactionId)
    /// son provistos por el handler después de crear las entidades correspondientes.
    /// </summary>
    public void Confirm(Guid? membershipAssignmentId, Guid cashTransactionId)
    {
        EnsureDraft();

        if (!_items.Any())
            throw new BusinessRuleViolationException(
                "SALE_NO_ITEMS", "La venta debe tener al menos un ítem para ser confirmada.");

        if (cashTransactionId == Guid.Empty)
            throw new BusinessRuleViolationException(
                "SALE_CASH_TRANSACTION_REQUIRED", "La transacción de caja es obligatoria al confirmar.");

        MembershipAssignmentId = membershipAssignmentId;
        CashTransactionId      = cashTransactionId;
        Status                 = SaleStatus.Confirmed;
    }

    // ── Anulación ─────────────────────────────────────────────────────────

    public void Void(DateTime voidedAtUtc, string reason)
    {
        if (Status != SaleStatus.Confirmed)
            throw new BusinessRuleViolationException(
                "SALE_VOID_INVALID",
                "Solo se puede anular una venta confirmada.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new BusinessRuleViolationException(
                "SALE_VOID_REASON_REQUIRED", "El motivo de anulación es obligatorio.");

        Status      = SaleStatus.Voided;
        VoidedAtUtc = voidedAtUtc;
        VoidReason  = reason.Trim();
    }

    // ── Consultas ─────────────────────────────────────────────────────────

    /// <summary>Devuelve el ítem Plan de la venta, o null si no hay ninguno.</summary>
    public SaleItem? GetPlanItem() => _items.FirstOrDefault(i => i.ItemType == SaleItemType.Plan);

    // ── Helpers privados ──────────────────────────────────────────────────

    private void EnsureDraft()
    {
        if (Status != SaleStatus.Draft)
            throw new BusinessRuleViolationException(
                "SALE_NOT_DRAFT",
                "Esta operación solo está disponible en ventas en estado Draft.");
    }

    private void RecalculateTotals()
    {
        SubtotalAmount = _items.Sum(i => i.Subtotal);
        DiscountAmount = _discounts.Sum(d => d.Amount);
        TotalAmount    = SubtotalAmount - DiscountAmount;
    }
}
