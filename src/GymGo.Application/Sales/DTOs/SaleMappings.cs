using GymGo.Domain.Cash;
using GymGo.Domain.Sales;

namespace GymGo.Application.Sales.DTOs;

public static class SaleMappings
{
    public static string ToLabel(this SaleStatus s) => s switch
    {
        SaleStatus.Draft     => "Borrador",
        SaleStatus.Confirmed => "Confirmada",
        SaleStatus.Voided    => "Anulada",
        _                    => s.ToString()
    };

    public static string ToLabel(this SaleItemType t) => t switch
    {
        SaleItemType.Plan         => "Plan",
        SaleItemType.Matricula    => "Matrícula",
        SaleItemType.ServicioExtra => "Servicio Extra",
        SaleItemType.Otro         => "Otro",
        _                         => t.ToString()
    };

    public static string ToLabel(this SaleDiscountType t) => t switch
    {
        SaleDiscountType.MontoFijo  => "Monto Fijo",
        SaleDiscountType.Porcentaje => "Porcentaje",
        _                           => t.ToString()
    };

    public static string ToLabel(this CashPaymentMethod m) => m switch
    {
        CashPaymentMethod.Efectivo      => "Efectivo",
        CashPaymentMethod.Tarjeta       => "Tarjeta",
        CashPaymentMethod.Transferencia => "Transferencia",
        _                               => m.ToString()
    };

    public static SaleItemDto ToDto(this SaleItem item, string? planName = null) =>
        new(
            item.Id,
            item.ItemType,
            item.ItemType.ToLabel(),
            item.Description,
            item.MembershipPlanId,
            planName,
            item.UnitPrice,
            item.Quantity,
            item.Subtotal
        );

    public static SaleDiscountDto ToDto(this SaleDiscount discount) =>
        new(
            discount.Id,
            discount.DiscountType,
            discount.DiscountType.ToLabel(),
            discount.Description,
            discount.Value,
            discount.Amount
        );
}
