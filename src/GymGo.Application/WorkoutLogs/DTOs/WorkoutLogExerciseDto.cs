using GymGo.Domain.WorkoutLogs;

namespace GymGo.Application.WorkoutLogs.DTOs;

/// <summary>
/// Representación de un ejercicio dentro de una sesión de entrenamiento.
/// </summary>
public sealed record WorkoutLogExerciseDto(
    Guid Id,
    Guid WorkoutLogId,
    string ExerciseName,
    MuscleGroup MuscleGroup,
    string MuscleGroupName,
    int SortOrder,
    int? Sets,
    int? Reps,
    decimal? WeightKg,
    int? DurationSeconds,
    decimal? DistanceMeters,
    string? Notes
);
