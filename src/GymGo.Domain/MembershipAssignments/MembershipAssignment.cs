using GymGo.Domain.Common;
using GymGo.Domain.Exceptions;

namespace GymGo.Domain.MembershipAssignments;

/// <summary>
/// Asignación de un plan de membresía a un socio.
/// Representa el contrato vigente (o histórico) entre el socio y el gimnasio.
///
/// Invariantes del dominio:
/// - MemberId y MembershipPlanId son inmutables una vez creada la asignación.
/// - EndDate = StartDate + durationDays (calculado al crear).
/// - AmountSnapshot preserva el monto al momento de la asignación;
///   cambios futuros en el plan no afectan asignaciones ya creadas.
/// - Solo se puede cancelar o congelar una asignación en estado Active.
/// - Solo se puede registrar pago en estado Pending o Overdue.
/// - El congelamiento solo está permitido si el plan lo habilitó (validado en el handler).
/// </summary>
public sealed class MembershipAssignment : AggregateRoot, IAuditable, ITenantScoped, ISoftDeletable
{
    // ── Tenant ────────────────────────────────────────────────────────────
    public Guid TenantId { get; set; }

    // ── Relaciones (IDs inmutables) ───────────────────────────────────────
    /// <summary>Socio al que se asigna la membresía.</summary>
    public Guid MemberId { get; private set; }

    /// <summary>Plan asignado.</summary>
    public Guid MembershipPlanId { get; private set; }

    // ── Período ───────────────────────────────────────────────────────────
    /// <summary>Fecha de inicio de la membresía.</summary>
    public DateOnly StartDate { get; private set; }

    /// <summary>
    /// Fecha de vencimiento calculada: StartDate + durationDays del plan.
    /// Puede extenderse si el socio congela la membresía.
    /// </summary>
    public DateOnly EndDate { get; private set; }

    // ── Snapshot comercial ────────────────────────────────────────────────
    /// <summary>
    /// Monto pagado al momento de la asignación en CLP.
    /// Preservado aunque el plan cambie de precio posteriormente.
    /// </summary>
    public decimal AmountSnapshot { get; private set; }

    // ── Estado ────────────────────────────────────────────────────────────
    /// <summary>Estado operacional de la asignación.</summary>
    public AssignmentStatus Status { get; private set; }

    /// <summary>Estado del pago asociado a esta asignación.</summary>
    public PaymentStatus PaymentStatus { get; private set; }

    /// <summary>Fecha en que se registró el pago. Null si aún no se pagó.</summary>
    public DateTime? PaidAtUtc { get; private set; }

    // ── Congelamiento ─────────────────────────────────────────────────────
    /// <summary>Fecha de inicio del congelamiento activo. Null si no está congelado.</summary>
    public DateOnly? FrozenSince { get; private set; }

    /// <summary>Días acumulados de congelamiento. Se suman a EndDate al descongelar.</summary>
    public int FrozenDaysAccumulated { get; private set; }

    // ── Observaciones ─────────────────────────────────────────────────────
    public string? Notes { get; private set; }

    // ── IAuditable ────────────────────────────────────────────────────────
    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedAtUtc { get; set; }
    public string? ModifiedBy { get; set; }

    // ── ISoftDeletable ────────────────────────────────────────────────────
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public string? DeletedBy { get; set; }

    // ── Constructor privado para EF Core ──────────────────────────────────
    private MembershipAssignment() { }

    private MembershipAssignment(
        Guid id, Guid tenantId, Guid memberId, Guid membershipPlanId,
        DateOnly startDate, DateOnly endDate, decimal amountSnapshot, string? notes)
        : base(id)
    {
        TenantId          = tenantId;
        MemberId          = memberId;
        MembershipPlanId  = membershipPlanId;
        StartDate         = startDate;
        EndDate           = endDate;
        AmountSnapshot    = amountSnapshot;
        Status            = AssignmentStatus.Active;
        PaymentStatus     = PaymentStatus.Pending;
        Notes             = notes;
        FrozenDaysAccumulated = 0;
    }

    // ── Factory ───────────────────────────────────────────────────────────

    /// <summary>
    /// Crea una nueva asignación de membresía.
    /// </summary>
    /// <param name="tenantId">Gimnasio.</param>
    /// <param name="memberId">Socio que recibe el plan.</param>
    /// <param name="membershipPlanId">Plan asignado.</param>
    /// <param name="planDurationDays">Duración del plan en días (tomado de MembershipPlan.DurationDays).</param>
    /// <param name="amountSnapshot">Monto del plan al momento de la asignación.</param>
    /// <param name="startDate">Fecha de inicio. Si es null, se usa la fecha de hoy.</param>
    /// <param name="notes">Observaciones opcionales.</param>
    public static MembershipAssignment Create(
        Guid tenantId,
        Guid memberId,
        Guid membershipPlanId,
        int planDurationDays,
        decimal amountSnapshot,
        DateOnly? startDate = null,
        string? notes = null)
    {
        if (tenantId == Guid.Empty)
            throw new BusinessRuleViolationException(
                "ASSIGNMENT_TENANT_REQUIRED", "El tenant es obligatorio.");

        if (memberId == Guid.Empty)
            throw new BusinessRuleViolationException(
                "ASSIGNMENT_MEMBER_REQUIRED", "El socio es obligatorio.");

        if (membershipPlanId == Guid.Empty)
            throw new BusinessRuleViolationException(
                "ASSIGNMENT_PLAN_REQUIRED", "El plan de membresía es obligatorio.");

        if (planDurationDays <= 0)
            throw new BusinessRuleViolationException(
                "ASSIGNMENT_DURATION_INVALID", "La duración del plan debe ser mayor a cero.");

        if (amountSnapshot <= 0)
            throw new BusinessRuleViolationException(
                "ASSIGNMENT_AMOUNT_INVALID", "El monto de la asignación debe ser mayor a cero.");

        if (notes?.Length > 500)
            throw new BusinessRuleViolationException(
                "ASSIGNMENT_NOTES_TOO_LONG", "Las observaciones no pueden superar los 500 caracteres.");

        var start = startDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var end   = start.AddDays(planDurationDays);

        return new MembershipAssignment(
            Guid.NewGuid(), tenantId, memberId, membershipPlanId,
            start, end, amountSnapshot,
            string.IsNullOrWhiteSpace(notes) ? null : notes.Trim());
    }

    // ── Comportamiento de pago ────────────────────────────────────────────

    /// <summary>
    /// Registra el pago de la membresía.
    /// Solo válido si el estado de pago es Pending o Overdue.
    /// </summary>
    public void RegisterPayment(DateTime paidAtUtc)
    {
        if (PaymentStatus == PaymentStatus.Paid)
            throw new BusinessRuleViolationException(
                "ASSIGNMENT_ALREADY_PAID", "Esta membresía ya tiene el pago registrado.");

        PaymentStatus = PaymentStatus.Paid;
        PaidAtUtc     = paidAtUtc;
    }

    /// <summary>
    /// Marca la asignación como morosa por falta de pago.
    /// Solo válido si el estado de pago es Pending.
    /// </summary>
    public void MarkOverdue()
    {
        if (PaymentStatus != PaymentStatus.Pending)
            throw new BusinessRuleViolationException(
                "ASSIGNMENT_OVERDUE_INVALID",
                "Solo se puede marcar como morosa una asignación con pago pendiente.");

        PaymentStatus = PaymentStatus.Overdue;
    }

    // ── Comportamiento de ciclo de vida ───────────────────────────────────

    /// <summary>Cancela la asignación antes de su vencimiento.</summary>
    public void Cancel()
    {
        if (Status != AssignmentStatus.Active)
            throw new BusinessRuleViolationException(
                "ASSIGNMENT_CANCEL_INVALID",
                "Solo se puede cancelar una asignación activa.");

        Status = AssignmentStatus.Cancelled;
    }

    /// <summary>Marca la asignación como vencida.</summary>
    public void Expire()
    {
        if (Status != AssignmentStatus.Active && Status != AssignmentStatus.Frozen)
            throw new BusinessRuleViolationException(
                "ASSIGNMENT_EXPIRE_INVALID",
                "Solo se puede vencer una asignación activa o congelada.");

        Status = AssignmentStatus.Expired;
    }

    // ── Congelamiento ─────────────────────────────────────────────────────

    /// <summary>
    /// Congela la membresía. La fecha de vencimiento se extenderá al descongelar.
    /// La validación de si el plan permite congelamiento se hace en el handler.
    /// </summary>
    public void Freeze(DateOnly frozenSince)
    {
        if (Status != AssignmentStatus.Active)
            throw new BusinessRuleViolationException(
                "ASSIGNMENT_FREEZE_INVALID",
                "Solo se puede congelar una membresía activa.");

        if (FrozenSince.HasValue)
            throw new BusinessRuleViolationException(
                "ASSIGNMENT_ALREADY_FROZEN",
                "La membresía ya está congelada.");

        Status      = AssignmentStatus.Frozen;
        FrozenSince = frozenSince;
    }

    /// <summary>
    /// Descongela la membresía extendiendo EndDate por los días congelados.
    /// </summary>
    public void Unfreeze(DateOnly today)
    {
        if (Status != AssignmentStatus.Frozen || !FrozenSince.HasValue)
            throw new BusinessRuleViolationException(
                "ASSIGNMENT_NOT_FROZEN",
                "La membresía no está congelada.");

        var frozenDays = today.DayNumber - FrozenSince.Value.DayNumber;
        FrozenDaysAccumulated += frozenDays;
        EndDate     = EndDate.AddDays(frozenDays);
        Status      = AssignmentStatus.Active;
        FrozenSince = null;
    }
}
