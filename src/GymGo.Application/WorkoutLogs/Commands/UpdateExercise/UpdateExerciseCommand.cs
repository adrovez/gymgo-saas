using GymGo.Domain.WorkoutLogs;
using MediatR;

namespace GymGo.Application.WorkoutLogs.Commands.UpdateExercise;

public sealed record UpdateExerciseCommand(
    Guid WorkoutLogId,
    Guid ExerciseId,
    string ExerciseName,
    MuscleGroup MuscleGroup,
    int SortOrder,
    int? ActualSets,
    int? ActualReps,
    decimal? ActualWeightKg,
    int? ActualDurationMinutes,
    int? ActualDistanceMeters,
    string? Notes
) : IRequest;
