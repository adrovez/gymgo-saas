using GymGo.Domain.Common;
using GymGo.Domain.Exceptions;

namespace GymGo.Domain.Sales;

/// <summary>
/// Descuento aplicado a una venta.
///
/// Invariantes:
/// - Description es obligatoria.
/// - Value > 0. Si Porcentaje, Value <= 100.
/// - Amount es el monto calculado en CLP (persiste para trazabilidad histórica).
/// - La validación de que la suma de descuentos no supere el subtotal
///   se realiza en Sale.RecalculateTotals().
/// </summary>
public sealed class SaleDiscount : Entity
{
    public Guid TenantId { get; private set; }
    public Guid SaleId { get; private set; }

    public SaleDiscountType DiscountType { get; private set; }
    public string Description { get; private set; } = default!;

    /// <summary>Monto fijo en CLP o porcentaje (ej: 10.5 para 10,5%).</summary>
    public decimal Value { get; private set; }

    /// <summary>Monto calculado en CLP. Siempre positivo.</summary>
    public decimal Amount { get; private set; }

    private SaleDiscount() { }

    private SaleDiscount(
        Guid id, Guid tenantId, Guid saleId,
        SaleDiscountType discountType, string description,
        decimal value, decimal amount)
        : base(id)
    {
        TenantId     = tenantId;
        SaleId       = saleId;
        DiscountType = discountType;
        Description  = description;
        Value        = value;
        Amount       = amount;
    }

    /// <summary>
    /// Crea un descuento y calcula Amount a partir del subtotal de la venta.
    /// </summary>
    internal static SaleDiscount Create(
        Guid tenantId,
        Guid saleId,
        SaleDiscountType discountType,
        string description,
        decimal value,
        decimal saleSubtotal)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new BusinessRuleViolationException(
                "SALE_DISCOUNT_DESCRIPTION_REQUIRED", "La descripción del descuento es obligatoria.");

        if (description.Trim().Length > 200)
            throw new BusinessRuleViolationException(
                "SALE_DISCOUNT_DESCRIPTION_TOO_LONG", "La descripción del descuento no puede superar los 200 caracteres.");

        if (value <= 0)
            throw new BusinessRuleViolationException(
                "SALE_DISCOUNT_VALUE_INVALID", "El valor del descuento debe ser mayor a cero.");

        if (discountType == SaleDiscountType.Porcentaje && value > 100)
            throw new BusinessRuleViolationException(
                "SALE_DISCOUNT_PCT_INVALID", "El porcentaje de descuento no puede superar el 100%.");

        var amount = discountType == SaleDiscountType.Porcentaje
            ? Math.Round(saleSubtotal * value / 100, 2)
            : value;

        if (amount <= 0)
            throw new BusinessRuleViolationException(
                "SALE_DISCOUNT_AMOUNT_ZERO", "El monto calculado del descuento debe ser mayor a cero.");

        return new SaleDiscount(
            Guid.NewGuid(), tenantId, saleId,
            discountType, description.Trim(), value, amount);
    }
}
