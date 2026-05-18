using GymGo.Domain.Common;
using GymGo.Domain.Exceptions;

namespace GymGo.Domain.WorkoutLogs;

public sealed class WorkoutPlanExercise : Entity
{
    public Guid WorkoutPlanDayId { get; private set; }

    public string ExerciseName { get; private set; } = default!;
    public MuscleGroup MuscleGroup { get; private set; }
    public int SortOrder { get; private set; }

    public int? PlannedSets { get; private set; }
    public int? PlannedReps { get; private set; }
    public decimal? PlannedWeightKg { get; private set; }
    public int? PlannedDurationMinutes { get; private set; }
    public int? PlannedDistanceMeters { get; private set; }

    public string? Notes { get; private set; }

    private WorkoutPlanExercise() { }

    private WorkoutPlanExercise(
        Guid id,
        Guid workoutPlanDayId,
        string exerciseName,
        MuscleGroup muscleGroup,
        int sortOrder,
        int? plannedSets,
        int? plannedReps,
        decimal? plannedWeightKg,
        int? plannedDurationMinutes,
        int? plannedDistanceMeters,
        string? notes)
        : base(id)
    {
        WorkoutPlanDayId       = workoutPlanDayId;
        ExerciseName           = exerciseName;
        MuscleGroup            = muscleGroup;
        SortOrder              = sortOrder;
        PlannedSets            = plannedSets;
        PlannedReps            = plannedReps;
        PlannedWeightKg        = plannedWeightKg;
        PlannedDurationMinutes = plannedDurationMinutes;
        PlannedDistanceMeters  = plannedDistanceMeters;
        Notes                  = notes;
    }

    public static WorkoutPlanExercise Create(
        Guid workoutPlanDayId,
        string exerciseName,
        MuscleGroup muscleGroup = MuscleGroup.NotSpecified,
        int sortOrder = 0,
        int? plannedSets = null,
        int? plannedReps = null,
        decimal? plannedWeightKg = null,
        int? plannedDurationMinutes = null,
        int? plannedDistanceMeters = null,
        string? notes = null)
    {
        if (workoutPlanDayId == Guid.Empty)
            throw new BusinessRuleViolationException(
                "PLANEXERCISE_DAY_REQUIRED",
                "El ejercicio planificado debe pertenecer a un día de rutina.");

        if (string.IsNullOrWhiteSpace(exerciseName))
            throw new BusinessRuleViolationException(
                "PLANEXERCISE_NAME_REQUIRED",
                "El nombre del ejercicio es obligatorio.");

        if (exerciseName.Length > 200)
            throw new BusinessRuleViolationException(
                "PLANEXERCISE_NAME_TOO_LONG",
                "El nombre del ejercicio no puede superar los 200 caracteres.");

        ValidateMetrics(plannedSets, plannedReps, plannedWeightKg, plannedDurationMinutes, plannedDistanceMeters);

        if (notes is not null && notes.Length > 500)
            throw new BusinessRuleViolationException(
                "PLANEXERCISE_NOTES_TOO_LONG",
                "Las notas del ejercicio no pueden superar los 500 caracteres.");

        return new WorkoutPlanExercise(
            Guid.NewGuid(),
            workoutPlanDayId,
            exerciseName.Trim(),
            muscleGroup,
            sortOrder,
            plannedSets,
            plannedReps,
            plannedWeightKg,
            plannedDurationMinutes,
            plannedDistanceMeters,
            string.IsNullOrWhiteSpace(notes) ? null : notes.Trim());
    }

    public void Update(
        string exerciseName,
        MuscleGroup muscleGroup,
        int sortOrder,
        int? plannedSets,
        int? plannedReps,
        decimal? plannedWeightKg,
        int? plannedDurationMinutes,
        int? plannedDistanceMeters,
        string? notes)
    {
        if (string.IsNullOrWhiteSpace(exerciseName))
            throw new BusinessRuleViolationException(
                "PLANEXERCISE_NAME_REQUIRED",
                "El nombre del ejercicio es obligatorio.");

        if (exerciseName.Length > 200)
            throw new BusinessRuleViolationException(
                "PLANEXERCISE_NAME_TOO_LONG",
                "El nombre del ejercicio no puede superar los 200 caracteres.");

        ValidateMetrics(plannedSets, plannedReps, plannedWeightKg, plannedDurationMinutes, plannedDistanceMeters);

        if (notes is not null && notes.Length > 500)
            throw new BusinessRuleViolationException(
                "PLANEXERCISE_NOTES_TOO_LONG",
                "Las notas del ejercicio no pueden superar los 500 caracteres.");

        ExerciseName           = exerciseName.Trim();
        MuscleGroup            = muscleGroup;
        SortOrder              = sortOrder;
        PlannedSets            = plannedSets;
        PlannedReps            = plannedReps;
        PlannedWeightKg        = plannedWeightKg;
        PlannedDurationMinutes = plannedDurationMinutes;
        PlannedDistanceMeters  = plannedDistanceMeters;
        Notes                  = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }

    private static void ValidateMetrics(
        int? sets, int? reps, decimal? weightKg, int? durationMinutes, int? distanceMeters)
    {
        if (sets.HasValue && sets.Value <= 0)
            throw new BusinessRuleViolationException("PLANEXERCISE_SETS_INVALID", "Las series deben ser mayor a cero.");

        if (reps.HasValue && reps.Value <= 0)
            throw new BusinessRuleViolationException("PLANEXERCISE_REPS_INVALID", "Las repeticiones deben ser mayor a cero.");

        if (weightKg.HasValue && weightKg.Value < 0)
            throw new BusinessRuleViolationException("PLANEXERCISE_WEIGHT_INVALID", "El peso no puede ser negativo.");

        if (durationMinutes.HasValue && durationMinutes.Value <= 0)
            throw new BusinessRuleViolationException("PLANEXERCISE_DURATION_INVALID", "La duración debe ser mayor a cero minutos.");

        if (distanceMeters.HasValue && distanceMeters.Value <= 0)
            throw new BusinessRuleViolationException("PLANEXERCISE_DISTANCE_INVALID", "La distancia debe ser mayor a cero metros.");
    }
}
