using GymGo.Domain.WorkoutLogs;
using MediatR;

namespace GymGo.Application.WorkoutPlans.Commands.CreateWorkoutPlan;

public sealed record CreateWorkoutPlanCommand(
    Guid MemberId,
    string Objective,
    DateOnly StartDate,
    DateOnly EndDate,
    string? Notes,
    decimal? InitialWeightKg,
    decimal? InitialHeightCm,
    decimal? InitialBodyFatPercentage
) : IRequest<Guid>;
