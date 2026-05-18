using MediatR;

namespace GymGo.Application.WorkoutPlans.Commands.DeleteWorkoutPlan;

public sealed record DeleteWorkoutPlanCommand(Guid Id) : IRequest;
