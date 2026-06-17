using GymGo.Domain.Common;
using GymGo.Domain.Exceptions;

namespace GymGo.Domain.Cash;

/// <summary>
/// Representa una transacción de caja: un ingreso o egreso de dinero en el gimnasio.
///
/// Invariantes:
/// - Amount siempre es positivo; el campo Type indica la dirección del dinero.
/// - Los egresos deben tener Description obligatoria (identificar el gasto).
/// - Los vínculos MemberId / MembershipAssignmentId solo aplican a ingresos.
/// - Las transacciones no se eliminan físicamente — se anulan con VoidReason.
/// </summary>
public sealed class CashTransaction : AggregateRoot, ITenantScoped
{
    // ── Tenant ────────────────────────────────────────────────────────────
    public Guid TenantId { get; set; }

    // ── Clasificación ─────────────────────────────────────────────────────
    public DateOnly Date { get; private set; }
    public CashTransactionType Type { get; private set; }
    public decimal Amount { get; private set; }
    public CashPaymentMethod PaymentMethod { get; private set; }
    public TransactionConcept Concept { get; private set; }

    // ── Descripción ───────────────────────────────────────────────────────
    public string? Description { get; private set; }

    // ── Vínculos (solo Ingresos) ──────────────────────────────────────────
    public Guid? MemberId { get; private set; }
    public Guid? MembershipAssignmentId { get; private set; }

    // ── Auditoría ─────────────────────────────────────────────────────────
    public Guid ProcessedByUserId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    // ── Anulación ─────────────────────────────────────────────────────────
    public bool IsVoided { get; private set; }
    public DateTime? VoidedAtUtc { get; private set; }
    public string? VoidReason { get; private set; }

    private static readonly TransactionConcept[] IncomeConcepts =
    [
        TransactionConcept.CuotaMembresia,
        TransactionConcept.Matricula,
        TransactionConcept.ProductoServicio,
        TransactionConcept.OtroIngreso
    ];

    private static readonly TransactionConcept[] ExpenseConcepts =
    [
        TransactionConcept.Servicios,
        TransactionConcept.Mantencion,
        TransactionConcept.Insumos,
        TransactionConcept.OtroEgreso
    ];

    private CashTransaction() { }

    private CashTransaction(
        Guid id, Guid tenantId,
        DateOnly date, CashTransactionType type, decimal amount,
        CashPaymentMethod paymentMethod, TransactionConcept concept,
        string? description, Guid? memberId, Guid? membershipAssignmentId,
        Guid processedByUserId, DateTime createdAtUtc)
        : base(id)
    {
        TenantId                = tenantId;
        Date                    = date;
        Type                    = type;
        Amount                  = amount;
        PaymentMethod           = paymentMethod;
        Concept                 = concept;
        Description             = description;
        MemberId                = memberId;
        MembershipAssignmentId  = membershipAssignmentId;
        ProcessedByUserId       = processedByUserId;
        CreatedAtUtc            = createdAtUtc;
    }

    // ── Factory ───────────────────────────────────────────────────────────

    public static CashTransaction Create(
        Guid tenantId,
        DateOnly date,
        CashTransactionType type,
        decimal amount,
        CashPaymentMethod paymentMethod,
        TransactionConcept concept,
        string? description,
        Guid? memberId,
        Guid? membershipAssignmentId,
        Guid processedByUserId,
        DateTime createdAtUtc)
    {
        if (amount <= 0)
            throw new BusinessRuleViolationException(
                "CASH_AMOUNT_INVALID", "El monto debe ser mayor a cero.");

        if (type == CashTransactionType.Egreso && string.IsNullOrWhiteSpace(description))
            throw new BusinessRuleViolationException(
                "CASH_DESCRIPTION_REQUIRED", "Los egresos deben llevar una descripción del gasto.");

        if (description?.Length > 500)
            throw new BusinessRuleViolationException(
                "CASH_DESCRIPTION_TOO_LONG", "La descripción no puede superar los 500 caracteres.");

        if (type == CashTransactionType.Ingreso && !IncomeConcepts.Contains(concept))
            throw new BusinessRuleViolationException(
                "CASH_CONCEPT_MISMATCH", "El concepto no corresponde a un ingreso.");

        if (type == CashTransactionType.Egreso && !ExpenseConcepts.Contains(concept))
            throw new BusinessRuleViolationException(
                "CASH_CONCEPT_MISMATCH", "El concepto no corresponde a un egreso.");

        if (type == CashTransactionType.Egreso && (memberId.HasValue || membershipAssignmentId.HasValue))
            throw new BusinessRuleViolationException(
                "CASH_EGRESO_NO_MEMBER", "Los egresos no pueden vincularse a un socio o membresía.");

        return new CashTransaction(
            Guid.NewGuid(), tenantId,
            date, type, amount, paymentMethod, concept,
            string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            memberId, membershipAssignmentId,
            processedByUserId, createdAtUtc);
    }

    // ── Comportamiento ────────────────────────────────────────────────────

    public void Void(DateTime voidedAtUtc, string reason)
    {
        if (IsVoided)
            throw new BusinessRuleViolationException(
                "CASH_ALREADY_VOIDED", "Esta transacción ya fue anulada.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new BusinessRuleViolationException(
                "CASH_VOID_REASON_REQUIRED", "El motivo de anulación es obligatorio.");

        IsVoided    = true;
        VoidedAtUtc = voidedAtUtc;
        VoidReason  = reason.Trim();
    }
}
