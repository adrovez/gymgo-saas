using MediatR;

namespace GymGo.Application.Equipment.Commands.CreateEquipment;

public sealed record CreateEquipmentCommand(
    string    Name,
    string?   Brand,
    string?   Model,
    string?   SerialNumber,
    DateOnly? PurchaseDate,
    string?   ImageUrl
) : IRequest<Guid>;
