using GymGo.Domain.Common;
using GymGo.Domain.Exceptions;

namespace GymGo.Domain.WorkoutLogs;

public sealed class WorkoutLog : AggregateRoot, IAuditable, ITenantScoped, ISoftDeletable
{
    public Guid TenantId { get; set; }
    public Guid MemberId { get; private set; }

    public Guid WorkoutPlanId { get; private set; }
    public Guid WorkoutPlanDayId { get; private set; }

    public DateOnly Date { get; private set; }
    public string? Notes { get; private set; }
    public WorkoutLogStatus Status { get; private set; }

    private readonly List<WorkoutLogExercise> _exercises = new();
    public IReadOnlyList<WorkoutLogExercise> Exercises => _exercises.AsReadOnly();

    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedAtUtc { get; set; }
    public string? ModifiedBy { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public string? DeletedBy { get; set; }

    private WorkoutLog() { }

    private WorkoutLog(
        Guid id,
        Guid tenantId,
        Guid memberId,
        Guid workoutPlanId,
        Guid workoutPlanDayId,
        DateOnly date,
        string? notes)
        : base(id)
    {
        TenantId         = tenantId;
        MemberId         = memberId;
        WorkoutPlanId    = workoutPlanId;
        WorkoutPlanDayId = workoutPlanDayId;
        Date             = date;
        Notes            = notes;
        Status           = WorkoutLogStatus.Draft;
    }

    public static WorkoutLog Create(
        Guid tenantId,
        Guid memberId,
        Guid workoutPlanId,
        Guid workoutPlanDayId,
        DateOnly? date = null,
        string? notes = null)
    {
        if (tenantId == Guid.Empty)
            throw new BusinessRuleViolationException("LOG_TENANT_REQUIRED", "El registro debe pertenecer a un gimnasio.");

        if (memberId == Guid.Empty)
            throw new BusinessRuleViolationException("LOG_MEMBER_REQUIRED", "El registro debe pertenecer a un socio.");

        if (workoutPlanId == Guid.Empty)
            throw new BusinessRuleViolationException("LOG_PLAN_REQUIRED", "El registro debe referenciar una rutina activa.");

        if (workoutPlanDayId == Guid.Empty)
            throw new BusinessRuleViolationException("LOG_PLANDAY_REQUIRED", "El registro debe referenciar un día de la rutina.");

        if (notes is not null && notes.Length > 1000)
            throw new BusinessRuleViolationException("LOG_NOTES_TOO_LONG", "Las observaciones no pueden superar los 1000 caracteres.");

        return new WorkoutLog(
            Guid.NewGuid(),
            tenantId,
            memberId,
            workoutPlanId,
            workoutPlanDayId,
            date ?? DateOnly.FromDateTime(DateTime.UtcNow),
            string.IsNullOrWhiteSpace(notes) ? null : notes.Trim());
    }

    public void UpdateNotes(string? notes)
    {
        EnsureNotCompleted("editar");

        if (notes is not null && notes.Length > 1000)
            throw new BusinessRuleViolationException("LOG_NOTES_TOO_LONG", "Las observaciones no pueden superar los 1000 caracteres.");

        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }

    public void Complete()
    {
        if (Status == WorkoutLogStatus.Completed)
            throw new BusinessRuleViolationException("LOG_ALREADY_COMPLETED", "La sesión ya fue marcada como completada.");

        if (_exercises.Count == 0)
            throw new BusinessRuleViolationException("LOG_NO_EXERCISES", "No se puede completar una sesión sin ejercicios registrados.");

        Status = WorkoutLogStatus.Completed;
    }

    public WorkoutLogExercise AddExercise(
        string exerciseName,
        MuscleGroup muscleGroup = MuscleGroup.NotSpecified,
        Guid? workoutPlanExerciseId = null,
        bool isExtra = false,
        int? actualSets = null,
        int? actualReps = null,
        decimal? actualWeightKg = null,
        int? actualDurationMinutes = null,
        int? actualDistanceMeters = null,
        string? notes = null)
    {
        EnsureNotCompleted("agregar ejercicios a");

        var sortOrder = _exercises.Count > 0
            ? _exercises.Max(e => e.SortOrder) + 1
            : 0;

        var exercise = WorkoutLogExercise.Create(
            workoutLogId:          Id,
            exerciseName:          exerciseName,
            muscleGroup:           muscleGroup,
            workoutPlanExerciseId: workoutPlanExerciseId,
            isExtra:               isExtra,
            sortOrder:             sortOrder,
            actualSets:            actualSets,
            actualReps:            actualReps,
            actualWeightKg:        actualWeightKg,
            actualDurationMinutes: actualDurationMinutes,
            actualDistanceMeters:  actualDistanceMeters,
            notes:                 notes);

        _exercises.Add(exercise);
        return exercise;
    }

    public void UpdateExercise(
        Guid exerciseId,
        string exerciseName,
        MuscleGroup muscleGroup,
        int sortOrder,
        int? actualSets,
        int? actualReps,
        decimal? actualWeightKg,
        int? actualDurationMinutes,
        int? actualDistanceMeters,
        string? notes)
    {
        EnsureNotCompleted("editar ejercicios de");

        var exercise = _exercises.FirstOrDefault(e => e.Id == exerciseId)
            ?? throw new NotFoundException("WorkoutLogExercise", exerciseId);

        exercise.Update(exerciseName, muscleGroup, sortOrder,
            actualSets, actualReps, actualWeightKg,
            actualDurationMinutes, actualDistanceMeters, notes);
    }

    public void RemoveExercise(Guid exerciseId)
    {
        EnsureNotCompleted("eliminar ejercicios de");

        var exercise = _exercises.FirstOrDefault(e => e.Id == exerciseId)
            ?? throw new NotFoundException("WorkoutLogExercise", exerciseId);

        _exercises.Remove(exercise);
    }

    private void EnsureNotCompleted(string action)
    {
        if (Status == WorkoutLogStatus.Completed)
            throw new BusinessRuleViolationException(
                "LOG_IS_COMPLETED",
                $"No se puede {action} una sesión ya completada.");
    }
}
