using MediatR;

namespace GymGo.Application.Sales.Commands.VoidSale;

/// <summary>
/// Anula una venta Confirmed.
/// Anula la CashTransaction asociada y cancela la MembershipAssignment (si existe).
/// </summary>
public sealed record VoidSaleCommand(Guid SaleId, string Reason) : IRequest;
