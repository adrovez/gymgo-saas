using GymGo.Domain.Cash;
using GymGo.Domain.Sales;

namespace GymGo.Application.Sales.DTOs;

/// <summary>Ítem de detalle de una venta.</summary>
public sealed record SaleItemDto(
    Guid Id,
    SaleItemType ItemType,
    string ItemTypeLabel,
    string Description,
    Guid? MembershipPlanId,
    string? PlanName,
    decimal UnitPrice,
    int Quantity,
    decimal Subtotal
);

/// <summary>Descuento aplicado a una venta.</summary>
public sealed record SaleDiscountDto(
    Guid Id,
    SaleDiscountType DiscountType,
    string DiscountTypeLabel,
    string Description,
    decimal Value,
    decimal Amount
);

/// <summary>Detalle completo de una venta, con ítems y descuentos.</summary>
public sealed record SaleDto(
    Guid Id,
    Guid TenantId,
    Guid MemberId,
    string MemberFullName,
    string MemberRut,
    DateOnly Date,
    SaleStatus Status,
    string StatusLabel,
    CashPaymentMethod PaymentMethod,
    string PaymentMethodLabel,
    decimal SubtotalAmount,
    decimal DiscountAmount,
    decimal TotalAmount,
    Guid? MembershipAssignmentId,
    Guid? CashTransactionId,
    string? Notes,
    Guid ProcessedByUserId,
    DateTime? VoidedAtUtc,
    string? VoidReason,
    DateTime CreatedAtUtc,
    string? CreatedBy,
    DateTime? ModifiedAtUtc,
    string? ModifiedBy,
    IReadOnlyList<SaleItemDto> Items,
    IReadOnlyList<SaleDiscountDto> Discounts
);

/// <summary>Vista resumida para listados.</summary>
public sealed record SaleSummaryDto(
    Guid Id,
    Guid MemberId,
    string MemberFullName,
    string MemberRut,
    DateOnly Date,
    SaleStatus Status,
    string StatusLabel,
    CashPaymentMethod PaymentMethod,
    string PaymentMethodLabel,
    decimal SubtotalAmount,
    decimal DiscountAmount,
    decimal TotalAmount,
    int ItemCount,
    DateTime CreatedAtUtc
);
