using MediatR;

namespace GymGo.Application.Sales.Commands.UpdateSaleItem;

public sealed record UpdateSaleItemCommand(
    Guid SaleId,
    Guid ItemId,
    string Description,
    decimal UnitPrice,
    int Quantity
) : IRequest;
