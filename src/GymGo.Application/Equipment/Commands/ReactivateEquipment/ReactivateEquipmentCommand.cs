using MediatR;

namespace GymGo.Application.Equipment.Commands.ReactivateEquipment;

public sealed record ReactivateEquipmentCommand(Guid Id) : IRequest;
