using MediatR;

namespace GymGo.Application.WorkoutLogs.Commands.UpdateWorkoutLog;

public sealed record UpdateWorkoutLogCommand(Guid Id, string? Notes) : IRequest;
