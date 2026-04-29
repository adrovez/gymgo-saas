using MediatR;

namespace GymGo.Application.GymClasses.Commands.DeactivateGymClass;

public sealed record DeactivateGymClassCommand(Guid Id) : IRequest;
