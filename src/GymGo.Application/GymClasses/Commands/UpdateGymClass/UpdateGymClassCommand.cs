using GymGo.Domain.GymClasses;
using MediatR;

namespace GymGo.Application.GymClasses.Commands.UpdateGymClass;

public sealed record UpdateGymClassCommand(
    Guid Id,
    string Name,
    string? Description,
    ClassCategory Category,
    string? Color,
    int DurationMinutes,
    int MaxCapacity
) : IRequest;
