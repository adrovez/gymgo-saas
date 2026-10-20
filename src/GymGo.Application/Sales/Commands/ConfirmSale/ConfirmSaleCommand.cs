using MediatR;

namespace GymGo.Application.Sales.Commands.ConfirmSale;

/// <summary>
/// Confirma una venta: calcula totales, crea la MembershipAssignment
/// (si hay ítem Plan) y la CashTransaction, todo en una sola transacción.
/// </summary>
public sealed record ConfirmSaleCommand(Guid SaleId) : IRequest;
