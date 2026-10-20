using GymGo.Domain.Cash;
using GymGo.Domain.Sales;
using MediatR;

namespace GymGo.Application.Sales.Commands.CheckoutSale;

public sealed record CheckoutSaleItemInput(
    SaleItemType ItemType,
    string Description,
    Guid? MembershipPlanId,
    decimal UnitPrice,
    int Quantity
);

public sealed record CheckoutSaleDiscountInput(
    SaleDiscountType DiscountType,
    string Description,
    decimal Value
);

/// <summary>
/// Crea y confirma una venta en un solo paso: Sale + SaleItems + descuento (opcional)
/// se persisten y confirman (generando MembershipAssignment y CashTransaction si corresponde)
/// en una única transacción de BD. No queda ningún Draft si la operación falla.
/// </summary>
public sealed record CheckoutSaleCommand(
    Guid MemberId,
    DateOnly Date,
    CashPaymentMethod PaymentMethod,
    string? Notes,
    IReadOnlyList<CheckoutSaleItemInput> Items,
    CheckoutSaleDiscountInput? Discount
) : IRequest<Guid>;
