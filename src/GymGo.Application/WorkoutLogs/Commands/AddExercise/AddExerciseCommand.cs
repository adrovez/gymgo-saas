using GymGo.Domain.WorkoutLogs;
using MediatR;

namespace GymGo.Application.WorkoutLogs.Commands.AddExercise;

public sealed record AddExerciseCommand(
    Guid WorkoutLogId,
    string ExerciseName,
    MuscleGroup MuscleGroup,
    Guid? WorkoutPlanExerciseId,
    bool IsExtra,
    int? ActualSets,
    int? ActualReps,
    decimal? ActualWeightKg,
    int? ActualDurationMinutes,
    int? ActualDistanceMeters,
    string? Notes
) : IRequest<Guid>;
