using GymGo.Domain.Common;
using GymGo.Domain.Exceptions;

namespace GymGo.Domain.WorkoutLogs;

/// <summary>
/// Registro de un ejercicio dentro de una sesión de entrenamiento.
/// Entidad hija de <see cref="WorkoutLog"/>; no implementa ITenantScoped
/// porque el aislamiento se garantiza transitivamente a través del WorkoutLog.
/// </summary>
public sealed class WorkoutLogExercise : Entity
{
    // ── Relación con la sesión ────────────────────────────────────────────
    /// <summary>Id de la rutina a la que pertenece este ejercicio.</summary>
    public Guid WorkoutLogId { get; private set; }

    // ── Ejercicio ─────────────────────────────────────────────────────────
    /// <summary>Nombre del ejercicio (ej: "Press de banca", "Sentadilla libre").</summary>
    public string ExerciseName { get; private set; } = default!;

    /// <summary>Grupo muscular principal trabajado.</summary>
    public MuscleGroup MuscleGroup { get; private set; }

    /// <summary>Posición del ejercicio dentro de la sesión (para ordenamiento).</summary>
    public int SortOrder { get; private set; }

    // ── Métricas ──────────────────────────────────────────────────────────
    /// <summary>Número de series realizadas (ej: 4).</summary>
    public int? Sets { get; private set; }

    /// <summary>Repeticiones por serie (ej: 10).</summary>
    public int? Reps { get; private set; }

    /// <summary>Peso utilizado en kg (ej: 80.0). Nulo para ejercicios sin peso externo.</summary>
    public decimal? WeightKg { get; private set; }

    /// <summary>Duración en segundos (para ejercicios de tiempo, planchas, etc.).</summary>
    public int? DurationSeconds { get; private set; }

    /// <summary>Distancia en metros (para ejercicios cardio: remo, bici, carrera).</summary>
    public decimal? DistanceMeters { get; private set; }

    // ── Observaciones ─────────────────────────────────────────────────────
    /// <summary>Notas específicas del ejercicio (ej: "con pausa en el fondo", "ROM completo").</summary>
    public string? Notes { get; private set; }

    // ── Constructor privado para EF Core ──────────────────────────────────
    private WorkoutLogExercise() { }

    private WorkoutLogExercise(
        Guid id,
        Guid workoutLogId,
        string exerciseName,
        MuscleGroup muscleGroup,
        int sortOrder,
        int? sets,
        int? reps,
        decimal? weightKg,
        int? durationSeconds,
        decimal? distanceMeters,
        string? notes)
        : base(id)
    {
        WorkoutLogId    = workoutLogId;
        ExerciseName    = exerciseName;
        MuscleGroup     = muscleGroup;
        SortOrder       = sortOrder;
        Sets            = sets;
        Reps            = reps;
        WeightKg        = weightKg;
        DurationSeconds = durationSeconds;
        DistanceMeters  = distanceMeters;
        Notes           = notes;
    }

    // ── Factory ───────────────────────────────────────────────────────────

    /// <summary>
    /// Crea un nuevo ejercicio para la sesión indicada.
    /// </summary>
    public static WorkoutLogExercise Create(
        Guid workoutLogId,
        string exerciseName,
        MuscleGroup muscleGroup = MuscleGroup.NotSpecified,
        int sortOrder = 0,
        int? sets = null,
        int? reps = null,
        decimal? weightKg = null,
        int? durationSeconds = null,
        decimal? distanceMeters = null,
        string? notes = null)
    {
        if (workoutLogId == Guid.Empty)
            throw new BusinessRuleViolationException(
                "EXERCISE_WORKOUTLOG_REQUIRED",
                "El ejercicio debe pertenecer a una rutina (WorkoutLogId requerido).");

        if (string.IsNullOrWhiteSpace(exerciseName))
            throw new BusinessRuleViolationException(
                "EXERCISE_NAME_REQUIRED",
                "El nombre del ejercicio es obligatorio.");

        if (exerciseName.Length > 200)
            throw new BusinessRuleViolationException(
                "EXERCISE_NAME_TOO_LONG",
                "El nombre del ejercicio no puede superar los 200 caracteres.");

        if (sets.HasValue && sets.Value <= 0)
            throw new BusinessRuleViolationException(
                "EXERCISE_SETS_INVALID",
                "El número de series debe ser mayor a cero.");

        if (reps.HasValue && reps.Value <= 0)
            throw new BusinessRuleViolationException(
                "EXERCISE_REPS_INVALID",
                "Las repeticiones deben ser mayores a cero.");

        if (weightKg.HasValue && weightKg.Value < 0)
            throw new BusinessRuleViolationException(
                "EXERCISE_WEIGHT_INVALID",
                "El peso no puede ser negativo.");

        if (durationSeconds.HasValue && durationSeconds.Value <= 0)
            throw new BusinessRuleViolationException(
                "EXERCISE_DURATION_INVALID",
                "La duración debe ser mayor a cero segundos.");

        if (distanceMeters.HasValue && distanceMeters.Value <= 0)
            throw new BusinessRuleViolationException(
                "EXERCISE_DISTANCE_INVALID",
                "La distancia debe ser mayor a cero metros.");

        if (notes is not null && notes.Length > 500)
            throw new BusinessRuleViolationException(
                "EXERCISE_NOTES_TOO_LONG",
                "Las notas del ejercicio no pueden superar los 500 caracteres.");

        return new WorkoutLogExercise(
            Guid.NewGuid(),
            workoutLogId,
            exerciseName.Trim(),
            muscleGroup,
            sortOrder,
            sets,
            reps,
            weightKg,
            durationSeconds,
            distanceMeters,
            string.IsNullOrWhiteSpace(notes) ? null : notes.Trim());
    }

    // ── Comportamiento ────────────────────────────────────────────────────

    /// <summary>Actualiza los datos del ejercicio.</summary>
    public void Update(
        string exerciseName,
        MuscleGroup muscleGroup,
        int sortOrder,
        int? sets,
        int? reps,
        decimal? weightKg,
        int? durationSeconds,
        decimal? distanceMeters,
        string? notes)
    {
        if (string.IsNullOrWhiteSpace(exerciseName))
            throw new BusinessRuleViolationException(
                "EXERCISE_NAME_REQUIRED",
                "El nombre del ejercicio es obligatorio.");

        if (exerciseName.Length > 200)
            throw new BusinessRuleViolationException(
                "EXERCISE_NAME_TOO_LONG",
                "El nombre del ejercicio no puede superar los 200 caracteres.");

        if (sets.HasValue && sets.Value <= 0)
            throw new BusinessRuleViolationException(
                "EXERCISE_SETS_INVALID",
                "El número de series debe ser mayor a cero.");

        if (reps.HasValue && reps.Value <= 0)
            throw new BusinessRuleViolationException(
                "EXERCISE_REPS_INVALID",
                "Las repeticiones deben ser mayores a cero.");

        if (weightKg.HasValue && weightKg.Value < 0)
            throw new BusinessRuleViolationException(
                "EXERCISE_WEIGHT_INVALID",
                "El peso no puede ser negativo.");

        if (durationSeconds.HasValue && durationSeconds.Value <= 0)
            throw new BusinessRuleViolationException(
                "EXERCISE_DURATION_INVALID",
                "La duración debe ser mayor a cero segundos.");

        if (distanceMeters.HasValue && distanceMeters.Value <= 0)
            throw new BusinessRuleViolationException(
                "EXERCISE_DISTANCE_INVALID",
                "La distancia debe ser mayor a cero metros.");

        if (notes is not null && notes.Length > 500)
            throw new BusinessRuleViolationException(
                "EXERCISE_NOTES_TOO_LONG",
                "Las notas del ejercicio no pueden superar los 500 caracteres.");

        ExerciseName    = exerciseName.Trim();
        MuscleGroup     = muscleGroup;
        SortOrder       = sortOrder;
        Sets            = sets;
        Reps            = reps;
        WeightKg        = weightKg;
        DurationSeconds = durationSeconds;
        DistanceMeters  = distanceMeters;
        Notes           = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }
}
