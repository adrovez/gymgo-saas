using GymGo.Domain.WorkoutLogs;
using MediatR;

namespace GymGo.Application.WorkoutPlans.Commands.UpdatePlanExercise;

public sealed record UpdatePlanExerciseCommand(
    Guid WorkoutPlanDayId,
    Guid ExerciseId,
    string ExerciseName,
    MuscleGroup MuscleGroup,
    int SortOrder,
    int? PlannedSets,
    int? PlannedReps,
    decimal? PlannedWeightKg,
    int? PlannedDurationMinutes,
    int? PlannedDistanceMeters,
    string? Notes
) : IRequest;
