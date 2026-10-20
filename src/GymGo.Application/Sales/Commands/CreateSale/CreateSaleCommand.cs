using GymGo.Domain.Cash;
using MediatR;

namespace GymGo.Application.Sales.Commands.CreateSale;

/// <summary>
/// Crea una venta en estado Draft para el socio indicado.
/// Retorna el Id de la nueva Sale.
/// </summary>
public sealed record CreateSaleCommand(
    Guid MemberId,
    DateOnly Date,
    CashPaymentMethod PaymentMethod,
    string? Notes
) : IRequest<Guid>;
