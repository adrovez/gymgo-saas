using MediatR;

namespace GymGo.Application.WorkoutLogs.Commands.CreateWorkoutLog;

public sealed record CreateWorkoutLogCommand(
    Guid MemberId,
    Guid WorkoutPlanId,
    Guid WorkoutPlanDayId,
    DateOnly? Date,
    string? Notes
) : IRequest<Guid>;
