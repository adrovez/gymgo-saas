using GymGo.Domain.Common;
using GymGo.Domain.Exceptions;

namespace GymGo.Domain.ClassReservations;

/// <summary>
/// Reserva de un socio para una sesión concreta de una clase.
///
/// Una "sesión" queda identificada por (ClassScheduleId + SessionDate):
/// el horario semanal recurrente más la fecha exacta en que ocurrirá.
///
/// Invariantes:
/// - MemberId y ClassScheduleId son inmutables.
/// - SessionDate no puede ser anterior a hoy (validado en el handler).
/// - SessionDate debe coincidir con el DayOfWeek del ClassSchedule (validado en el handler).
/// - Un socio no puede tener dos reservas activas para la misma sesión
///   (la unicidad la verifica el handler antes de persistir).
/// - MemberFullName es un snapshot del nombre al momento de la reserva.
/// - Solo se puede cancelar una reserva en estado Active.
/// </summary>
public sealed class ClassReservation : AggregateRoot, IAuditable, ITenantScoped
{
    // ── Tenant ────────────────────────────────────────────────────────────
    public Guid TenantId { get; set; }

    // ── Relaciones ────────────────────────────────────────────────────────
    public Guid MemberId { get; private set; }
    public Guid ClassScheduleId { get; private set; }

    // ── Datos de la sesión ────────────────────────────────────────────────
    /// <summary>Fecha concreta de la sesión reservada.</summary>
    public DateOnly SessionDate { get; private set; }

    /// <summary>Timestamp en que se creó la reserva (UTC).</summary>
    public DateTime ReservedAtUtc { get; private set; }

    /// <summary>Nombre completo del socio al momento de la reserva (snapshot).</summary>
    public string MemberFullName { get; private set; } = string.Empty;

    /// <summary>Observaciones opcionales.</summary>
    public string? Notes { get; private set; }

    // ── Estado ────────────────────────────────────────────────────────────
    public ReservationStatus Status { get; private set; }

    /// <summary>Timestamp de cancelación. Null si está activa.</summary>
    public DateTime? CancelledAtUtc { get; private set; }

    /// <summary>Usuario que canceló. Null si está activa.</summary>
    public string? CancelledBy { get; private set; }

    /// <summary>Motivo de cancelación. Opcional.</summary>
    public string? CancelReason { get; private set; }

    // ── IAuditable ────────────────────────────────────────────────────────
    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedAtUtc { get; set; }
    public string? ModifiedBy { get; set; }

    // ── Constructor privado para EF Core ──────────────────────────────────
    private ClassReservation() { }

    private ClassReservation(
        Guid id, Guid tenantId, Guid memberId, Guid classScheduleId,
        DateOnly sessionDate, DateTime reservedAtUtc,
        string memberFullName, string? notes)
        : base(id)
    {
        TenantId        = tenantId;
        MemberId        = memberId;
        ClassScheduleId = classScheduleId;
        SessionDate     = sessionDate;
        ReservedAtUtc   = reservedAtUtc;
        MemberFullName  = memberFullName;
        Notes           = notes;
        Status          = ReservationStatus.Active;
    }

    // ── Factory ───────────────────────────────────────────────────────────

    /// <summary>
    /// Crea una nueva reserva para una sesión de clase.
    /// Las validaciones de capacidad, duplicado y sesión futura se realizan en el handler.
    /// </summary>
    public static ClassReservation Create(
        Guid tenantId,
        Guid memberId,
        Guid classScheduleId,
        DateOnly sessionDate,
        DateTime reservedAtUtc,
        string memberFullName,
        string? notes = null)
    {
        if (tenantId == Guid.Empty)
            throw new BusinessRuleViolationException(
                "RESERVATION_TENANT_REQUIRED", "El tenant es obligatorio.");

        if (memberId == Guid.Empty)
            throw new BusinessRuleViolationException(
                "RESERVATION_MEMBER_REQUIRED", "El socio es obligatorio.");

        if (classScheduleId == Guid.Empty)
            throw new BusinessRuleViolationException(
                "RESERVATION_SCHEDULE_REQUIRED", "El horario es obligatorio.");

        if (string.IsNullOrWhiteSpace(memberFullName))
            throw new BusinessRuleViolationException(
                "RESERVATION_MEMBER_NAME_REQUIRED", "El nombre del socio es obligatorio.");

        if (notes is not null && notes.Length > 500)
            throw new BusinessRuleViolationException(
                "RESERVATION_NOTES_TOO_LONG", "Las observaciones no pueden superar los 500 caracteres.");

        return new ClassReservation(
            Guid.NewGuid(), tenantId, memberId, classScheduleId,
            sessionDate, reservedAtUtc,
            memberFullName.Trim(),
            string.IsNullOrWhiteSpace(notes) ? null : notes.Trim());
    }

    // ── Comportamiento ────────────────────────────────────────────────────

    /// <summary>Cancela la reserva. Solo válido desde estado Active.</summary>
    public void Cancel(
        ReservationStatus cancelStatus,
        DateTime cancelledAtUtc,
        string cancelledBy,
        string? reason = null)
    {
        if (Status != ReservationStatus.Active)
            throw new BusinessRuleViolationException(
                "RESERVATION_CANCEL_INVALID",
                "Solo se puede cancelar una reserva activa.");

        if (cancelStatus != ReservationStatus.CancelledByMember
         && cancelStatus != ReservationStatus.CancelledByStaff)
            throw new BusinessRuleViolationException(
                "RESERVATION_CANCEL_STATUS_INVALID",
                "El estado de cancelación no es válido.");

        Status          = cancelStatus;
        CancelledAtUtc  = cancelledAtUtc;
        CancelledBy     = cancelledBy;
        CancelReason    = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
    }

    /// <summary>Marca la reserva como No Show (socio no se presentó).</summary>
    public void MarkNoShow(DateTime utcNow, string markedBy)
    {
        if (Status != ReservationStatus.Active)
            throw new BusinessRuleViolationException(
                "RESERVATION_NOSHOW_INVALID",
                "Solo se puede marcar como no-show una reserva activa.");

        Status         = ReservationStatus.NoShow;
        CancelledAtUtc = utcNow;
        CancelledBy    = markedBy;
    }
}
