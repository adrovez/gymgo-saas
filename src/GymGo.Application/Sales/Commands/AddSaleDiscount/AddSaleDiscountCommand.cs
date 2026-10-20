using GymGo.Domain.Sales;
using MediatR;

namespace GymGo.Application.Sales.Commands.AddSaleDiscount;

/// <summary>
/// Agrega un descuento a una venta en Draft.
/// Retorna el Id del nuevo SaleDiscount.
/// </summary>
public sealed record AddSaleDiscountCommand(
    Guid SaleId,
    SaleDiscountType DiscountType,
    string Description,
    decimal Value
) : IRequest<Guid>;
