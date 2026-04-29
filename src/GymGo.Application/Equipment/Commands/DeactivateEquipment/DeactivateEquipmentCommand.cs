using MediatR;

namespace GymGo.Application.Equipment.Commands.DeactivateEquipment;

public sealed record DeactivateEquipmentCommand(Guid Id) : IRequest;
