using GymGo.Domain.WorkoutLogs;
using MediatR;

namespace GymGo.Application.WorkoutPlans.Commands.AddPlanDay;

public sealed record AddPlanDayCommand(
    Guid WorkoutPlanId,
    WorkoutDayOfWeek DayOfWeek,
    string? Notes
) : IRequest<Guid>;
