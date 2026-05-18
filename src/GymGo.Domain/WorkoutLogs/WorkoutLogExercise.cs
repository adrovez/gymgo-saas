using GymGo.Domain.Common;
using GymGo.Domain.Exceptions;

namespace GymGo.Domain.WorkoutLogs;

public sealed class WorkoutLogExercise : Entity
{
    public Guid WorkoutLogId { get; private set; }

    // Null si es ejercicio extra no planificado
    public Guid? WorkoutPlanExerciseId { get; private set; }

    public string ExerciseName { get; private set; } = default!;
    public MuscleGroup MuscleGroup { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsExtra { get; private set; }

    public int? ActualSets { get; private set; }
    public int? ActualReps { get; private set; }
    public decimal? ActualWeightKg { get; private set; }
    public int? ActualDurationMinutes { get; private set; }
    public int? ActualDistanceMeters { get; private set; }

    public string? Notes { get; private set; }

    private WorkoutLogExercise() { }

    private WorkoutLogExercise(
        Guid id,
        Guid workoutLogId,
        Guid? workoutPlanExerciseId,
        string exerciseName,
        MuscleGroup muscleGroup,
        int sortOrder,
        bool isExtra,
        int? actualSets,
        int? actualReps,
        decimal? actualWeightKg,
        int? actualDurationMinutes,
        int? actualDistanceMeters,
        string? notes)
        : base(id)
    {
        WorkoutLogId           = workoutLogId;
        WorkoutPlanExerciseId  = workoutPlanExerciseId;
        ExerciseName           = exerciseName;
        MuscleGroup            = muscleGroup;
        SortOrder              = sortOrder;
        IsExtra                = isExtra;
        ActualSets             = actualSets;
        ActualReps             = actualReps;
        ActualWeightKg         = actualWeightKg;
        ActualDurationMinutes  = actualDurationMinutes;
        ActualDistanceMeters   = actualDistanceMeters;
        Notes                  = notes;
    }

    public static WorkoutLogExercise Create(
        Guid workoutLogId,
        string exerciseName,
        MuscleGroup muscleGroup = MuscleGroup.NotSpecified,
        Guid? workoutPlanExerciseId = null,
        bool isExtra = false,
        int sortOrder = 0,
        int? actualSets = null,
        int? actualReps = null,
        decimal? actualWeightKg = null,
        int? actualDurationMinutes = null,
        int? actualDistanceMeters = null,
        string? notes = null)
    {
        if (workoutLogId == Guid.Empty)
            throw new BusinessRuleViolationException("LOGEXERCISE_LOG_REQUIRED", "El ejercicio debe pertenecer a una sesión.");

        if (string.IsNullOrWhiteSpace(exerciseName))
            throw new BusinessRuleViolationException("LOGEXERCISE_NAME_REQUIRED", "El nombre del ejercicio es obligatorio.");

        if (exerciseName.Length > 200)
            throw new BusinessRuleViolationException("LOGEXERCISE_NAME_TOO_LONG", "El nombre del ejercicio no puede superar los 200 caracteres.");

        ValidateMetrics(actualSets, actualReps, actualWeightKg, actualDurationMinutes, actualDistanceMeters);

        if (notes is not null && notes.Length > 500)
            throw new BusinessRuleViolationException("LOGEXERCISE_NOTES_TOO_LONG", "Las notas no pueden superar los 500 caracteres.");

        return new WorkoutLogExercise(
            Guid.NewGuid(),
            workoutLogId,
            workoutPlanExerciseId,
            exerciseName.Trim(),
            muscleGroup,
            sortOrder,
            isExtra,
            actualSets,
            actualReps,
            actualWeightKg,
            actualDurationMinutes,
            actualDistanceMeters,
            string.IsNullOrWhiteSpace(notes) ? null : notes.Trim());
    }

    public void Update(
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
        if (string.IsNullOrWhiteSpace(exerciseName))
            throw new BusinessRuleViolationException("LOGEXERCISE_NAME_REQUIRED", "El nombre del ejercicio es obligatorio.");

        if (exerciseName.Length > 200)
            throw new BusinessRuleViolationException("LOGEXERCISE_NAME_TOO_LONG", "El nombre del ejercicio no puede superar los 200 caracteres.");

        ValidateMetrics(actualSets, actualReps, actualWeightKg, actualDurationMinutes, actualDistanceMeters);

        if (notes is not null && notes.Length > 500)
            throw new BusinessRuleViolationException("LOGEXERCISE_NOTES_TOO_LONG", "Las notas no pueden superar los 500 caracteres.");

        ExerciseName          = exerciseName.Trim();
        MuscleGroup           = muscleGroup;
        SortOrder             = sortOrder;
        ActualSets            = actualSets;
        ActualReps            = actualReps;
        ActualWeightKg        = actualWeightKg;
        ActualDurationMinutes = actualDurationMinutes;
        ActualDistanceMeters  = actualDistanceMeters;
        Notes                 = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }

    private static void ValidateMetrics(
        int? sets, int? reps, decimal? weightKg, int? durationMinutes, int? distanceMeters)
    {
        if (sets.HasValue && sets.Value <= 0)
            throw new BusinessRuleViolationException("LOGEXERCISE_SETS_INVALID", "Las series deben ser mayor a cero.");

        if (reps.HasValue && reps.Value <= 0)
            throw new BusinessRuleViolationException("LOGEXERCISE_REPS_INVALID", "Las repeticiones deben ser mayor a cero.");

        if (weightKg.HasValue && weightKg.Value < 0)
            throw new BusinessRuleViolationException("LOGEXERCISE_WEIGHT_INVALID", "El peso no puede ser negativo.");

        if (durationMinutes.HasValue && durationMinutes.Value <= 0)
            throw new BusinessRuleViolationException("LOGEXERCISE_DURATION_INVALID", "La duración debe ser mayor a cero minutos.");

        if (distanceMeters.HasValue && distanceMeters.Value <= 0)
            throw new BusinessRuleViolationException("LOGEXERCISE_DISTANCE_INVALID", "La distancia debe ser mayor a cero metros.");
    }
}
