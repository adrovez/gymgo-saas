using GymGo.Domain.Sales;
using MediatR;

namespace GymGo.Application.Sales.Commands.AddSaleItem;

/// <summary>
/// Agrega un ítem a una venta en Draft.
/// Retorna el Id del nuevo SaleItem.
/// </summary>
public sealed record AddSaleItemCommand(
    Guid SaleId,
    SaleItemType ItemType,
    string Description,
    Guid? MembershipPlanId,
    decimal UnitPrice,
    int Quantity
) : IRequest<Guid>;
