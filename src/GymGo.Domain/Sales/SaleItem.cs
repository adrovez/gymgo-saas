using GymGo.Domain.Common;
using GymGo.Domain.Exceptions;

namespace GymGo.Domain.Sales;

/// <summary>
/// Línea de detalle dentro de una venta.
/// Representa un concepto cobrable: plan, matrícula, servicio extra u otro.
///
/// Invariantes:
/// - Description es obligatoria.
/// - UnitPrice > 0. Quantity >= 1.
/// - Si ItemType = Plan, MembershipPlanId es obligatorio.
/// - Subtotal = UnitPrice × Quantity (calculado).
/// </summary>
public sealed class SaleItem : Entity
{
    public Guid TenantId { get; private set; }
    public Guid SaleId { get; private set; }

    public SaleItemType ItemType { get; private set; }
    public string Description { get; private set; } = default!;

    /// <summary>Referencia al plan. Solo poblado si ItemType = Plan.</summary>
    public Guid? MembershipPlanId { get; private set; }

    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }

    /// <summary>UnitPrice × Quantity.</summary>
    public decimal Subtotal => UnitPrice * Quantity;

    private SaleItem() { }

    private SaleItem(
        Guid id, Guid tenantId, Guid saleId,
        SaleItemType itemType, string description, Guid? membershipPlanId,
        decimal unitPrice, int quantity)
        : base(id)
    {
        TenantId         = tenantId;
        SaleId           = saleId;
        ItemType         = itemType;
        Description      = description;
        MembershipPlanId = membershipPlanId;
        UnitPrice        = unitPrice;
        Quantity         = quantity;
    }

    internal static SaleItem Create(
        Guid tenantId,
        Guid saleId,
        SaleItemType itemType,
        string description,
        Guid? membershipPlanId,
        decimal unitPrice,
        int quantity)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new BusinessRuleViolationException(
                "SALE_ITEM_DESCRIPTION_REQUIRED", "La descripción del ítem es obligatoria.");

        if (description.Trim().Length > 200)
            throw new BusinessRuleViolationException(
                "SALE_ITEM_DESCRIPTION_TOO_LONG", "La descripción del ítem no puede superar los 200 caracteres.");

        if (unitPrice <= 0)
            throw new BusinessRuleViolationException(
                "SALE_ITEM_PRICE_INVALID", "El precio unitario debe ser mayor a cero.");

        if (quantity < 1)
            throw new BusinessRuleViolationException(
                "SALE_ITEM_QUANTITY_INVALID", "La cantidad debe ser al menos 1.");

        if (itemType == SaleItemType.Plan && !membershipPlanId.HasValue)
            throw new BusinessRuleViolationException(
                "SALE_ITEM_PLAN_ID_REQUIRED", "Un ítem de tipo Plan requiere indicar el plan de membresía.");

        if (itemType != SaleItemType.Plan && membershipPlanId.HasValue)
            throw new BusinessRuleViolationException(
                "SALE_ITEM_PLAN_ID_NOT_ALLOWED", "Solo un ítem de tipo Plan puede referenciar un plan de membresía.");

        return new SaleItem(
            Guid.NewGuid(), tenantId, saleId,
            itemType, description.Trim(), membershipPlanId,
            unitPrice, quantity);
    }

    internal void Update(string description, decimal unitPrice, int quantity)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new BusinessRuleViolationException(
                "SALE_ITEM_DESCRIPTION_REQUIRED", "La descripción del ítem es obligatoria.");

        if (description.Trim().Length > 200)
            throw new BusinessRuleViolationException(
                "SALE_ITEM_DESCRIPTION_TOO_LONG", "La descripción del ítem no puede superar los 200 caracteres.");

        if (unitPrice <= 0)
            throw new BusinessRuleViolationException(
                "SALE_ITEM_PRICE_INVALID", "El precio unitario debe ser mayor a cero.");

        if (quantity < 1)
            throw new BusinessRuleViolationException(
                "SALE_ITEM_QUANTITY_INVALID", "La cantidad debe ser al menos 1.");

        Description = description.Trim();
        UnitPrice   = unitPrice;
        Quantity    = quantity;
    }
}
