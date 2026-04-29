using GymGo.Domain.Common;
using GymGo.Domain.Exceptions;

namespace GymGo.Domain.GymClasses;

/// <summary>
/// Slot semanal recurrente de una clase (ej. Spinning — Lunes 07:00).
/// Pertenece a un GymClass y define cuándo se dicta esa clase cada semana.
///
/// Invariantes:
/// - DayOfWeek entre 0 (Domingo) y 6 (Sábado).
/// - EndTime debe ser posterior a StartTime.
/// - MaxCapacity positivo si se especifica.
/// </summary>
public sealed class ClassSchedule : Entity, IAuditable, ITenantScoped, ISoftDeletable
{
    // ── Tenant y relación ─────────────────────────────────────────────────
    public Guid TenantId { get; set; }
    public Guid GymClassId { get; private set; }

    // ── Horario ───────────────────────────────────────────────────────────
    /// <summary>0 = Domingo … 6 = Sábado (convención .NET DayOfWeek).</summary>
    public DayOfWeek DayOfWeek { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }

    // ── Datos operacionales ───────────────────────────────────────────────
    public string? InstructorName { get; private set; }
    public string? Room { get; private set; }

    /// <summary>
    /// Capacidad máxima del slot. Si es null, hereda GymClass.MaxCapacity.
    /// </summary>
    public int? MaxCapacity { get; private set; }

    // ── Estado ────────────────────────────────────────────────────────────
    public bool IsActive { get; private set; }

    // ── IAuditable ────────────────────────────────────────────────────────
    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedAtUtc { get; set; }
    public string? ModifiedBy { get; set; }

    // ── ISoftDeletable ────────────────────────────────────────────────────
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public string? DeletedBy { get; set; }

    // ── Navegación inversa ────────────────────────────────────────────────
    public GymClass GymClass { get; private set; } = null!;

    // ── Constructor privado para EF Core ──────────────────────────────────
    private ClassSchedule() { }

    private ClassSchedule(
        Guid id, Guid tenantId, Guid gymClassId,
        DayOfWeek dayOfWeek, TimeOnly startTime, TimeOnly endTime,
        string? instructorName, string? room, int? maxCapacity)
        : base(id)
    {
        TenantId       = tenantId;
        GymClassId     = gymClassId;
        DayOfWeek      = dayOfWeek;
        StartTime      = startTime;
        EndTime        = endTime;
        InstructorName = instructorName;
        Room           = room;
        MaxCapacity    = maxCapacity;
        IsActive       = true;
    }

    // ── Factory ───────────────────────────────────────────────────────────

    public static ClassSchedule Create(
        Guid tenantId,
        Guid gymClassId,
        DayOfWeek dayOfWeek,
        TimeOnly startTime,
        TimeOnly endTime,
        string? instructorName,
        string? room,
        int? maxCapacity)
    {
        Validate(tenantId, gymClassId, dayOfWeek, startTime, endTime, maxCapacity);

        return new ClassSchedule(
            Guid.NewGuid(), tenantId, gymClassId,
            dayOfWeek, startTime, endTime,
            Trim(instructorName), Trim(room),
            maxCapacity);
    }

    // ── Comportamiento ────────────────────────────────────────────────────

    public void Update(
        DayOfWeek dayOfWeek,
        TimeOnly startTime,
        TimeOnly endTime,
        string? instructorName,
        string? room,
        int? maxCapacity)
    {
        Validate(TenantId, GymClassId, dayOfWeek, startTime, endTime, maxCapacity);

        DayOfWeek      = dayOfWeek;
        StartTime      = startTime;
        EndTime        = endTime;
        InstructorName = Trim(instructorName);
        Room           = Trim(room);
        MaxCapacity    = maxCapacity;
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new BusinessRuleViolationException(
                "SCHEDULE_ALREADY_INACTIVE", "El horario ya está inactivo.");
        IsActive = false;
    }

    public void Reactivate()
    {
        if (IsActive)
            throw new BusinessRuleViolationException(
                "SCHEDULE_ALREADY_ACTIVE", "El horario ya está activo.");
        IsActive = true;
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private static void Validate(
        Guid tenantId, Guid gymClassId,
        DayOfWeek dayOfWeek, TimeOnly startTime, TimeOnly endTime,
        int? maxCapacity)
    {
        if (tenantId == Guid.Empty)
            throw new BusinessRuleViolationException(
                "SCHEDULE_TENANT_REQUIRED", "El tenant es obligatorio.");

        if (gymClassId == Guid.Empty)
            throw new BusinessRuleViolationException(
                "SCHEDULE_CLASS_REQUIRED", "La clase es obligatoria.");

        if ((int)dayOfWeek < 0 || (int)dayOfWeek > 6)
            throw new BusinessRuleViolationException(
                "SCHEDULE_DAY_INVALID", "El día de la semana no es válido.");

        if (endTime <= startTime)
            throw new BusinessRuleViolationException(
                "SCHEDULE_TIME_RANGE_INVALID", "La hora de término debe ser posterior a la hora de inicio.");

        if (maxCapacity.HasValue && maxCapacity.Value <= 0)
            throw new BusinessRuleViolationException(
                "SCHEDULE_CAPACITY_INVALID", "La capacidad máxima debe ser mayor a cero.");
    }

    private static string? Trim(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
