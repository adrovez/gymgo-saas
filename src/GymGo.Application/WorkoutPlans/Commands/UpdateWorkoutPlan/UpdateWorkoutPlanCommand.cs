using MediatR;

namespace GymGo.Application.WorkoutPlans.Commands.UpdateWorkoutPlan;

public sealed record UpdateWorkoutPlanCommand(
    Guid Id,
    string Objective,
    DateOnly StartDate,
    DateOnly EndDate,
    string? Notes,
    decimal? InitialWeightKg,
    decimal? InitialHeightCm,
    decimal? InitialBodyFatPercentage
) : IRequest;
