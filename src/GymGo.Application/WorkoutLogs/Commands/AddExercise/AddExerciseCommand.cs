using GymGo.Domain.WorkoutLogs;
using MediatR;

namespace GymGo.Application.WorkoutLogs.Commands.AddExercise;

/// <summary>
/// Agrega un ejercicio a una sesión de entrenamiento existente (solo en Draft).
/// </summary>
public sealed record AddExerciseCommand(
    Guid WorkoutLogId,
    string ExerciseName,
    MuscleGroup MuscleGroup,
    int? Sets,
    int? Reps,
    decimal? WeightKg,
    int? DurationSeconds,
    decimal? DistanceMeters,
    string? Notes
) : IRequest<Guid>;
