using GymGo.Domain.Common;
using GymGo.Domain.Exceptions;

namespace GymGo.Domain.WorkoutLogs;

public sealed class WorkoutPlan : AggregateRoot, IAuditable, ITenantScoped, ISoftDeletable
{
    public Guid TenantId { get; set; }
    public Guid MemberId { get; private set; }

    public string Objective { get; private set; } = default!;
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public string? Notes { get; private set; }

    public decimal? InitialWeightKg { get; private set; }
    public decimal? InitialHeightCm { get; private set; }
    public decimal? InitialBodyFatPercentage { get; private set; }

    public WorkoutPlanStatus Status { get; private set; }

    private readonly List<WorkoutPlanDay> _days = new();
    public IReadOnlyList<WorkoutPlanDay> Days => _days.AsReadOnly();

    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedAtUtc { get; set; }
    public string? ModifiedBy { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public string? DeletedBy { get; set; }

    private WorkoutPlan() { }

    private WorkoutPlan(
        Guid id,
        Guid tenantId,
        Guid memberId,
        string objective,
        DateOnly startDate,
        DateOnly endDate,
        string? notes,
        decimal? initialWeightKg,
        decimal? initialHeightCm,
        decimal? initialBodyFatPercentage)
        : base(id)
    {
        TenantId                 = tenantId;
        MemberId                 = memberId;
        Objective                = objective;
        StartDate                = startDate;
        EndDate                  = endDate;
        Notes                    = notes;
        InitialWeightKg          = initialWeightKg;
        InitialHeightCm          = initialHeightCm;
        InitialBodyFatPercentage = initialBodyFatPercentage;
        Status                   = WorkoutPlanStatus.Active;
    }

    public static WorkoutPlan Create(
        Guid tenantId,
        Guid memberId,
        string objective,
        DateOnly startDate,
        DateOnly endDate,
        string? notes = null,
        decimal? initialWeightKg = null,
        decimal? initialHeightCm = null,
        decimal? initialBodyFatPercentage = null)
    {
        if (tenantId == Guid.Empty)
            throw new BusinessRuleViolationException("PLAN_TENANT_REQUIRED", "La rutina debe pertenecer a un gimnasio.");

        if (memberId == Guid.Empty)
            throw new BusinessRuleViolationException("PLAN_MEMBER_REQUIRED", "La rutina debe asignarse a un socio.");

        if (string.IsNullOrWhiteSpace(objective))
            throw new BusinessRuleViolationException("PLAN_OBJECTIVE_REQUIRED", "El objetivo de la rutina es obligatorio.");

        if (objective.Length > 500)
            throw new BusinessRuleViolationException("PLAN_OBJECTIVE_TOO_LONG", "El objetivo no puede superar los 500 caracteres.");

        if (endDate < startDate)
            throw new BusinessRuleViolationException("PLAN_DATES_INVALID", "La fecha de fin debe ser posterior o igual a la fecha de inicio.");

        ValidatePhysicalMetrics(initialWeightKg, initialHeightCm, initialBodyFatPercentage);

        if (notes is not null && notes.Length > 1000)
            throw new BusinessRuleViolationException("PLAN_NOTES_TOO_LONG", "Las observaciones no pueden superar los 1000 caracteres.");

        return new WorkoutPlan(
            Guid.NewGuid(),
            tenantId,
            memberId,
            objective.Trim(),
            startDate,
            endDate,
            string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            initialWeightKg,
            initialHeightCm,
            initialBodyFatPercentage);
    }

    public void Update(
        string objective,
        DateOnly startDate,
        DateOnly endDate,
        string? notes,
        decimal? initialWeightKg,
        decimal? initialHeightCm,
        decimal? initialBodyFatPercentage)
    {
        EnsureNotClosed("editar");

        if (string.IsNullOrWhiteSpace(objective))
            throw new BusinessRuleViolationException("PLAN_OBJECTIVE_REQUIRED", "El objetivo de la rutina es obligatorio.");

        if (objective.Length > 500)
            throw new BusinessRuleViolationException("PLAN_OBJECTIVE_TOO_LONG", "El objetivo no puede superar los 500 caracteres.");

        if (endDate < startDate)
            throw new BusinessRuleViolationException("PLAN_DATES_INVALID", "La fecha de fin debe ser posterior o igual a la fecha de inicio.");

        ValidatePhysicalMetrics(initialWeightKg, initialHeightCm, initialBodyFatPercentage);

        if (notes is not null && notes.Length > 1000)
            throw new BusinessRuleViolationException("PLAN_NOTES_TOO_LONG", "Las observaciones no pueden superar los 1000 caracteres.");

        Objective                = objective.Trim();
        StartDate                = startDate;
        EndDate                  = endDate;
        Notes                    = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        InitialWeightKg          = initialWeightKg;
        InitialHeightCm          = initialHeightCm;
        InitialBodyFatPercentage = initialBodyFatPercentage;
    }

    public void Complete()
    {
        if (Status != WorkoutPlanStatus.Active)
            throw new BusinessRuleViolationException("PLAN_NOT_ACTIVE", "Solo se puede completar una rutina activa.");

        Status = WorkoutPlanStatus.Completed;
    }

    public void Cancel()
    {
        if (Status != WorkoutPlanStatus.Active)
            throw new BusinessRuleViolationException("PLAN_NOT_ACTIVE", "Solo se puede cancelar una rutina activa.");

        Status = WorkoutPlanStatus.Cancelled;
    }

    public WorkoutPlanDay AddDay(WorkoutDayOfWeek dayOfWeek, string? notes = null)
    {
        EnsureNotClosed("agregar días a");

        if (_days.Any(d => d.DayOfWeek == dayOfWeek))
            throw new BusinessRuleViolationException(
                "PLAN_DAY_ALREADY_EXISTS",
                $"La rutina ya tiene configurado el día {dayOfWeek}.");

        var day = WorkoutPlanDay.Create(Id, dayOfWeek, notes);
        _days.Add(day);
        return day;
    }

    public void RemoveDay(Guid dayId)
    {
        EnsureNotClosed("eliminar días de");

        var day = _days.FirstOrDefault(d => d.Id == dayId)
            ?? throw new NotFoundException("WorkoutPlanDay", dayId);

        _days.Remove(day);
    }

    private void EnsureNotClosed(string action)
    {
        if (Status != WorkoutPlanStatus.Active)
            throw new BusinessRuleViolationException(
                "PLAN_IS_CLOSED",
                $"No se puede {action} una rutina que no está activa.");
    }

    private static void ValidatePhysicalMetrics(decimal? weightKg, decimal? heightCm, decimal? bodyFatPct)
    {
        if (weightKg.HasValue && weightKg.Value <= 0)
            throw new BusinessRuleViolationException("PLAN_WEIGHT_INVALID", "El peso debe ser mayor a cero.");

        if (heightCm.HasValue && heightCm.Value <= 0)
            throw new BusinessRuleViolationException("PLAN_HEIGHT_INVALID", "La estatura debe ser mayor a cero.");

        if (bodyFatPct.HasValue && (bodyFatPct.Value < 0 || bodyFatPct.Value > 100))
            throw new BusinessRuleViolationException("PLAN_BODYFAT_INVALID", "El porcentaje de grasa debe estar entre 0 y 100.");
    }
}
