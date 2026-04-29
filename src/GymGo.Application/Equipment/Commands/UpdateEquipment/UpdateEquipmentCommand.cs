using MediatR;

namespace GymGo.Application.Equipment.Commands.UpdateEquipment;

public sealed record UpdateEquipmentCommand(
    Guid      Id,
    string    Name,
    string?   Brand,
    string?   Model,
    string?   SerialNumber,
    DateOnly? PurchaseDate,
    string?   ImageUrl
) : IRequest;
