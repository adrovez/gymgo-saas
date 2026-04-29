using GymGo.Domain.Common;
using GymGo.Domain.Exceptions;

namespace GymGo.Domain.ClassAttendances;

/// <summary>
/// Registro de asistencia de un socio a una sesión concreta de una clase.
///
/// Una "sesión" queda identificada por (ClassScheduleId + SessionDate):
/// el horario semanal recurrente más la fecha exacta en que ocurre.
///
/// Invariantes:
/// - MemberId y ClassScheduleId son obligatorios.
/// - SessionDate no puede ser futura.
/// - MemberFullName es un snapshot del nombre en el momento del check-in.
/// - No se permite más de un check-in del mismo socio en la misma sesión
///   (la unicidad física está garantizada en DB; la regla de negocio la
///   verificamos antes de persistir en el handler).
/// </summary>
public sealed class ClassAttendance : AggregateRoot, IAuditable, ITenantScoped
{
    // ── Tenant ────────────────────────────────────────────────────────────
    public Guid TenantId { get; set; }

    // ── Relaciones ────────────────────────────────────────────────────────
    public Guid MemberId { get; private set; }
    public Guid ClassScheduleId { get; private set; }

    // ── Datos de la sesión ────────────────────────────────────────────────
    /// <summary>Fecha de la sesión (solo la parte de fecha, UTC).</summary>
    public DateOnly SessionDate { get; private set; }

    /// <summary>Timestamp exacto del check-in (UTC).</summary>
    public DateTime CheckedInAtUtc { get; private set; }

    /// <summary>Método utilizado para el check-in.</summary>
    public CheckInMethod CheckInMethod { get; private set; }

    /// <summary>
    /// Nombre completo del socio en el momento del check-in (snapshot).
    /// Permite mostrar el historial aunque el socio cambie de nombre.
    /// </summary>
    public string MemberFullName { get; private set; } = string.Empty;

    /// <summary>Observaciones opcionales de la recepcionista.</summary>
    public string? Notes { get; private set; }

    // ── IAuditable ────────────────────────────────────────────────────────
    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedAtUtc { get; set; }
    public string? ModifiedBy { get; set; }

    // ── Constructor privado para EF Core ──────────────────────────────────
    private ClassAttendance() { }

    private ClassAttendance(
        Guid id,
        Guid tenantId,
        Guid memberId,
        Guid classScheduleId,
        DateOnly sessionDate,
        DateTime checkedInAtUtc,
        CheckInMethod checkInMethod,
        string memberFullName,
        string? notes)
        : base(id)
    {
        TenantId        = tenantId;
        MemberId        = memberId;
        ClassScheduleId = classScheduleId;
        SessionDate     = sessionDate;
        CheckedInAtUtc  = checkedInAtUtc;
        CheckInMethod   = checkInMethod;
        MemberFullName  = memberFullName;
        Notes           = notes;
    }

    // ── Factory ───────────────────────────────────────────────────────────

    /// <summary>
    /// Registra el check-in de un socio a una sesión de clase.
    /// </summary>
    /// <param name="tenantId">Gimnasio activo.</param>
    /// <param name="memberId">Id del socio.</param>
    /// <param name="classScheduleId">Id del horario semanal.</param>
    /// <param name="sessionDate">Fecha concreta de la sesión.</param>
    /// <param name="checkedInAtUtc">Momento exacto del check-in (UTC).</param>
    /// <param name="checkInMethod">Manual o QR.</param>
    /// <param name="memberFullName">Nombre completo del socio (snapshot).</param>
    /// <param name="notes">Observaciones opcionales.</param>
    public static ClassAttendance Create(
        Guid tenantId,
        Guid memberId,
        Guid classScheduleId,
        DateOnly sessionDate,
        DateTime checkedInAtUtc,
        CheckInMethod checkInMethod,
        string memberFullName,
        string? notes = null)
    {
        if (tenantId == Guid.Empty)
            throw new BusinessRuleViolationException(
                "ATTENDANCE_TENANT_REQUIRED", "El tenant es obligatorio.");

        if (memberId == Guid.Empty)
            throw new BusinessRuleViolationException(
                "ATTENDANCE_MEMBER_REQUIRED", "El socio es obligatorio.");

        if (classScheduleId == Guid.Empty)
            throw new BusinessRuleViolationException(
                "ATTENDANCE_SCHEDULE_REQUIRED", "El horario es obligatorio.");

        if (sessionDate > DateOnly.FromDateTime(DateTime.UtcNow))
            throw new BusinessRuleViolationException(
                "ATTENDANCE_DATE_FUTURE", "La fecha de la sesión no puede ser futura.");

        if (string.IsNullOrWhiteSpace(memberFullName))
            throw new BusinessRuleViolationException(
                "ATTENDANCE_MEMBER_NAME_REQUIRED", "El nombre del socio es obligatorio.");

        if (notes is not null && notes.Length > 500)
            throw new BusinessRuleViolationException(
                "ATTENDANCE_NOTES_TOO_LONG", "Las observaciones no pueden superar los 500 caracteres.");

        return new ClassAttendance(
            Guid.NewGuid(),
            tenantId,
            memberId,
            classScheduleId,
            sessionDate,
            checkedInAtUtc,
            checkInMethod,
            memberFullName.Trim(),
            string.IsNullOrWhiteSpace(notes) ? null : notes.Trim());
    }
}
