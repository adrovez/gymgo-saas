using MediatR;

namespace GymGo.Application.WorkoutPlans.Commands.RemovePlanDay;

public sealed record RemovePlanDayCommand(Guid WorkoutPlanId, Guid DayId) : IRequest;
