using GymGo.Domain.GymClasses;
using MediatR;

namespace GymGo.Application.GymClasses.Commands.CreateGymClass;

public sealed record CreateGymClassCommand(
    string Name,
    string? Description,
    ClassCategory Category,
    string? Color,
    int DurationMinutes,
    int MaxCapacity
) : IRequest<Guid>;
