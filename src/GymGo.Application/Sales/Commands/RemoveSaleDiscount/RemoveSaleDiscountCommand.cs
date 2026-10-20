using MediatR;

namespace GymGo.Application.Sales.Commands.RemoveSaleDiscount;

public sealed record RemoveSaleDiscountCommand(Guid SaleId, Guid DiscountId) : IRequest;
