using GymGo.Domain.WorkoutLogs;
using MediatR;

namespace GymGo.Application.WorkoutPlans.Commands.AddPlanExercise;

public sealed record AddPlanExerciseCommand(
    Guid WorkoutPlanDayId,
    string ExerciseName,
    MuscleGroup MuscleGroup,
    int? PlannedSets,
    int? PlannedReps,
    decimal? PlannedWeightKg,
    int? PlannedDurationMinutes,
    int? PlannedDistanceMeters,
    string? Notes
) : IRequest<Guid>;
