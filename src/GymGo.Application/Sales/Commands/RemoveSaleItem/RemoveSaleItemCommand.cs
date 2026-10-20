using MediatR;

namespace GymGo.Application.Sales.Commands.RemoveSaleItem;

public sealed record RemoveSaleItemCommand(Guid SaleId, Guid ItemId) : IRequest;
