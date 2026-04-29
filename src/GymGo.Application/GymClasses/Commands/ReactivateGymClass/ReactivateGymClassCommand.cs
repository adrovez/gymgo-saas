using MediatR;

namespace GymGo.Application.GymClasses.Commands.ReactivateGymClass;

public sealed record ReactivateGymClassCommand(Guid Id) : IRequest;
