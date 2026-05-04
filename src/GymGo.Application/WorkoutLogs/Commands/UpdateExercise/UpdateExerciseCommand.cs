using GymGo.Domain.WorkoutLogs;
using MediatR;

namespace GymGo.Application.WorkoutLogs.Commands.UpdateExercise;

/// <summary>
/// Actualiza los datos de un ejercicio dentro de una sesión de entrenamiento.
/// </summary>
public sealed record UpdateExerciseCommand(
    Guid WorkoutLogId,
    Guid ExerciseId,
    string ExerciseName,
    MuscleGroup MuscleGroup,
    int SortOrder,
    int? Sets,
    int? Reps,
    decimal? WeightKg,
    int? DurationSeconds,
    decimal? DistanceMeters,
    string? Notes
) : IRequest;
