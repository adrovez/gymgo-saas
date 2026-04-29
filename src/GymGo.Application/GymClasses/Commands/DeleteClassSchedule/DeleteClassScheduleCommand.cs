using MediatR;

namespace GymGo.Application.GymClasses.Commands.DeleteClassSchedule;

public sealed record DeleteClassScheduleCommand(Guid Id) : IRequest;
