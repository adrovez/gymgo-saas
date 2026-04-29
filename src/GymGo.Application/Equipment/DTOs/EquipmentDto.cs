namespace GymGo.Application.Equipment.DTOs;

/// <summary>Detalle completo de una máquina.</summary>
public sealed record EquipmentDto(
    Guid      Id,
    Guid      TenantId,
    string    Name,
    string?   Brand,
    string?   Model,
    string?   SerialNumber,
    DateOnly? PurchaseDate,
    string?   ImageUrl,
    bool      IsActive,
    DateTime  CreatedAtUtc,
    string?   CreatedBy,
    DateTime? ModifiedAtUtc,
    string?   ModifiedBy
);

/// <summary>Resumen de máquina para listados.</summary>
public sealed record EquipmentSummaryDto(
    Guid      Id,
    string    Name,
    string?   Brand,
    string?   Model,
    string?   SerialNumber,
    DateOnly? PurchaseDate,
    string?   ImageUrl,
    bool      IsActive
);
