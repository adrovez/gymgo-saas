namespace GymGo.Domain.Sales;

/// <summary>
/// Tipo de descuento aplicado a una venta.
/// Persistido como INT en la base de datos.
/// </summary>
public enum SaleDiscountType
{
    /// <summary>Monto fijo en CLP. Amount = Value.</summary>
    MontoFijo = 0,

    /// <summary>Porcentaje sobre el subtotal. Amount = SubtotalAmount × Value / 100.</summary>
    Porcentaje = 1
}
