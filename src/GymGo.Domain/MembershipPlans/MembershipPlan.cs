using GymGo.Domain.Common;
using GymGo.Domain.Exceptions;

namespace GymGo.Domain.MembershipPlans;

/// <summary>
/// Plan de membresía que un gimnasio ofrece a sus socios.
/// Define las condiciones de acceso (días, horario) y el precio total del período.
///
/// Invariantes del dominio:
/// - Nombre es obligatorio.
/// - Monto debe ser mayor a cero.
/// - DiasPorSemana debe estar entre 1 y 7.
/// - Si DiasRigidos = true, al menos un día de la semana debe estar marcado
///   y DiasPorSemana debe coincidir con la cantidad de días marcados.
/// - Si HorarioLibre = false, HoraDesde y HoraHasta son obligatorias
///   y HoraDesde debe ser anterior a HoraHasta.
/// - DuracionDias se deriva automáticamente de la Periodicidad.
/// </summary>
public sealed class MembershipPlan : AggregateRoot, IAuditable, ITenantScoped, ISoftDeletable
{
    // ── Tenant ────────────────────────────────────────────────────────────
    /// <inheritdoc/>
    public Guid TenantId { get; set; }

    // ── Identidad comercial ───────────────────────────────────────────────
    /// <summary>Nombre comercial del plan. Ej: "Plan Mensual Full", "Plan Trimestral 3x Semana".</summary>
    public string Name { get; private set; } = default!;

    /// <summary>Descripción opcional del plan para mostrar al socio.</summary>
    public string? Description { get; private set; }

    // ── Periodicidad y duración ───────────────────────────────────────────
    /// <summary>Ciclo del plan: Mensual, Trimestral, Semestral o Anual.</summary>
    public Periodicity Periodicity { get; private set; }

    /// <summary>
    /// Duración en días del plan. Derivado de <see cref="Periodicity"/>:
    /// Mensual=30, Trimestral=90, Semestral=180, Anual=365.
    /// Se usa al asignar el plan a un socio para calcular la fecha de vencimiento.
    /// </summary>
    public int DurationDays { get; private set; }

    // ── Asistencia ────────────────────────────────────────────────────────
    /// <summary>
    /// Número referencial de días de asistencia por semana.
    /// Ej: 7 = todos los días, 3 = tres veces por semana.
    /// </summary>
    public int DaysPerWeek { get; private set; }

    /// <summary>
    /// Indica si los días de asistencia son fijos (true) o libres (false).
    /// Cuando es false, el socio elige qué días asistir dentro del límite de DaysPerWeek.
    /// Cuando es true, los campos Monday–Sunday definen exactamente qué días son válidos.
    /// </summary>
    public bool FixedDays { get; private set; }

    // Días válidos — sólo aplican si FixedDays = true
    public bool Monday    { get; private set; }
    public bool Tuesday   { get; private set; }
    public bool Wednesday { get; private set; }
    public bool Thursday  { get; private set; }
    public bool Friday    { get; private set; }
    public bool Saturday  { get; private set; }
    public bool Sunday    { get; private set; }

    // ── Horario ───────────────────────────────────────────────────────────
    /// <summary>
    /// Indica si el acceso es a cualquier hora del día (true)
    /// o restringido a un rango horario definido (false).
    /// </summary>
    public bool FreeSchedule { get; private set; }

    /// <summary>Hora de inicio del acceso. Sólo aplica si FreeSchedule = false.</summary>
    public TimeOnly? TimeFrom { get; private set; }

    /// <summary>Hora de fin del acceso. Sólo aplica si FreeSchedule = false.</summary>
    public TimeOnly? TimeTo { get; private set; }

    // ── Comercial ─────────────────────────────────────────────────────────
    /// <summary>Monto total del plan en CLP. Precio completo del período, no mensual.</summary>
    public decimal Amount { get; private set; }

    /// <summary>Indica si el socio puede pausar temporalmente el plan.</summary>
    public bool AllowsFreezing { get; private set; }

    // ── Ciclo de vida del plan ────────────────────────────────────────────
    /// <summary>Si false, el plan no está disponible para nuevas asignaciones.</summary>
    public bool IsActive { get; private set; }

    /// <summary>Fecha en que el plan fue desactivado. Null si está activo.</summary>
    public DateTime? DeactivatedAtUtc { get; private set; }

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
    private MembershipPlan() { }

    private MembershipPlan(
        Guid id,
        Guid tenantId,
        string name,
        string? description,
        Periodicity periodicity,
        int daysPerWeek,
        bool fixedDays,
        bool monday, bool tuesday, bool wednesday,
        bool thursday, bool friday, bool saturday, bool sunday,
        bool freeSchedule,
        TimeOnly? timeFrom,
        TimeOnly? timeTo,
        decimal amount,
        bool allowsFreezing)
        : base(id)
    {
        TenantId      = tenantId;
        Name          = name;
        Description   = description;
        Periodicity   = periodicity;
        DurationDays  = ResolveDurationDays(periodicity);
        DaysPerWeek   = daysPerWeek;
        FixedDays     = fixedDays;
        Monday        = monday;
        Tuesday       = tuesday;
        Wednesday     = wednesday;
        Thursday      = thursday;
        Friday        = friday;
        Saturday      = saturday;
        Sunday        = sunday;
        FreeSchedule  = freeSchedule;
        TimeFrom      = timeFrom;
        TimeTo        = timeTo;
        Amount        = amount;
        AllowsFreezing = allowsFreezing;
        IsActive      = true;
    }

    // ── Factory ───────────────────────────────────────────────────────────

    /// <summary>
    /// Crea un nuevo plan de membresía aplicando todas las reglas de dominio.
    /// </summary>
    public static MembershipPlan Create(
        Guid tenantId,
        string name,
        string? description,
        Periodicity periodicity,
        int daysPerWeek,
        bool fixedDays,
        bool monday, bool tuesday, bool wednesday,
        bool thursday, bool friday, bool saturday, bool sunday,
        bool freeSchedule,
        TimeOnly? timeFrom,
        TimeOnly? timeTo,
        decimal amount,
        bool allowsFreezing = false)
    {
        Validate(tenantId, name, daysPerWeek, fixedDays,
            monday, tuesday, wednesday, thursday, friday, saturday, sunday,
            freeSchedule, timeFrom, timeTo, amount);

        return new MembershipPlan(
            Guid.NewGuid(), tenantId,
            name.Trim(), string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            periodicity, daysPerWeek, fixedDays,
            monday, tuesday, wednesday, thursday, friday, saturday, sunday,
            freeSchedule, timeFrom, timeTo,
            amount, allowsFreezing);
    }

    // ── Comportamiento ────────────────────────────────────────────────────

    /// <summary>Actualiza los datos del plan.</summary>
    public void Update(
        string name,
        string? description,
        Periodicity periodicity,
        int daysPerWeek,
        bool fixedDays,
        bool monday, bool tuesday, bool wednesday,
        bool thursday, bool friday, bool saturday, bool sunday,
        bool freeSchedule,
        TimeOnly? timeFrom,
        TimeOnly? timeTo,
        decimal amount,
        bool allowsFreezing)
    {
        Validate(TenantId, name, daysPerWeek, fixedDays,
            monday, tuesday, wednesday, thursday, friday, saturday, sunday,
            freeSchedule, timeFrom, timeTo, amount);

        Name          = name.Trim();
        Description   = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Periodicity   = periodicity;
        DurationDays  = ResolveDurationDays(periodicity);
        DaysPerWeek   = daysPerWeek;
        FixedDays     = fixedDays;
        Monday        = monday;
        Tuesday       = tuesday;
        Wednesday     = wednesday;
        Thursday      = thursday;
        Friday        = friday;
        Saturday      = saturday;
        Sunday        = sunday;
        FreeSchedule  = freeSchedule;
        TimeFrom      = timeFrom;
        TimeTo        = timeTo;
        Amount        = amount;
        AllowsFreezing = allowsFreezing;
    }

    /// <summary>
    /// Desactiva el plan. Los socios que ya lo tienen asignado no se ven afectados,
    /// pero no se puede asignar a nuevos socios.
    /// </summary>
    public void Deactivate(DateTime utcNow)
    {
        if (!IsActive)
            throw new BusinessRuleViolationException(
                "PLAN_ALREADY_INACTIVE",
                "El plan ya se encuentra inactivo.");

        IsActive = false;
        DeactivatedAtUtc = utcNow;
    }

    /// <summary>Reactiva un plan previamente desactivado.</summary>
    public void Reactivate()
    {
        IsActive = true;
        DeactivatedAtUtc = null;
    }

    // ── Helpers privados ──────────────────────────────────────────────────

    /// <summary>Devuelve la duración en días según la periodicidad.</summary>
    public static int ResolveDurationDays(Periodicity periodicity) => periodicity switch
    {
        Periodicity.Monthly   => 30,
        Periodicity.Quarterly => 90,
        Periodicity.Biannual  => 180,
        Periodicity.Annual    => 365,
        _ => throw new BusinessRuleViolationException(
            "PLAN_PERIODICITY_INVALID",
            $"La periodicidad '{periodicity}' no es válida.")
    };

    private static void Validate(
        Guid tenantId,
        string name,
        int daysPerWeek,
        bool fixedDays,
        bool monday, bool tuesday, bool wednesday,
        bool thursday, bool friday, bool saturday, bool sunday,
        bool freeSchedule,
        TimeOnly? timeFrom,
        TimeOnly? timeTo,
        decimal amount)
    {
        if (tenantId == Guid.Empty)
            throw new BusinessRuleViolationException(
                "PLAN_TENANT_REQUIRED",
                "El plan debe pertenecer a un gimnasio.");

        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessRuleViolationException(
                "PLAN_NAME_REQUIRED",
                "El nombre del plan es obligatorio.");

        if (name.Trim().Length > 150)
            throw new BusinessRuleViolationException(
                "PLAN_NAME_TOO_LONG",
                "El nombre del plan no puede superar los 150 caracteres.");

        if (daysPerWeek is < 1 or > 7)
            throw new BusinessRuleViolationException(
                "PLAN_DAYS_PER_WEEK_INVALID",
                "Los días por semana deben estar entre 1 y 7.");

        if (fixedDays)
        {
            var markedDays = CountMarkedDays(monday, tuesday, wednesday, thursday, friday, saturday, sunday);

            if (markedDays == 0)
                throw new BusinessRuleViolationException(
                    "PLAN_FIXED_DAYS_REQUIRED",
                    "Si el plan tiene días rígidos, debe marcar al menos un día de la semana.");

            if (markedDays != daysPerWeek)
                throw new BusinessRuleViolationException(
                    "PLAN_DAYS_MISMATCH",
                    $"Los días marcados ({markedDays}) no coinciden con DiasPorSemana ({daysPerWeek}).");
        }

        if (!freeSchedule)
        {
            if (timeFrom is null)
                throw new BusinessRuleViolationException(
                    "PLAN_TIME_FROM_REQUIRED",
                    "La hora de inicio es obligatoria cuando el horario no es libre.");

            if (timeTo is null)
                throw new BusinessRuleViolationException(
                    "PLAN_TIME_TO_REQUIRED",
                    "La hora de fin es obligatoria cuando el horario no es libre.");

            if (timeFrom >= timeTo)
                throw new BusinessRuleViolationException(
                    "PLAN_TIME_RANGE_INVALID",
                    "La hora de inicio debe ser anterior a la hora de fin.");
        }

        if (amount <= 0)
            throw new BusinessRuleViolationException(
                "PLAN_AMOUNT_INVALID",
                "El monto del plan debe ser mayor a cero.");
    }

    private static int CountMarkedDays(
        bool monday, bool tuesday, bool wednesday,
        bool thursday, bool friday, bool saturday, bool sunday)
    {
        var count = 0;
        if (monday)    count++;
        if (tuesday)   count++;
        if (wednesday) count++;
        if (thursday)  count++;
        if (friday)    count++;
        if (saturday)  count++;
        if (sunday)    count++;
        return count;
    }
}
