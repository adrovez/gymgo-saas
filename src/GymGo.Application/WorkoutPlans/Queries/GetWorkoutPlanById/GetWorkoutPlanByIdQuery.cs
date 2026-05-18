using GymGo.Application.WorkoutPlans.DTOs;
using MediatR;

namespace GymGo.Application.WorkoutPlans.Queries.GetWorkoutPlanById;

public sealed record GetWorkoutPlanByIdQuery(Guid Id) : IRequest<WorkoutPlanDto>;
