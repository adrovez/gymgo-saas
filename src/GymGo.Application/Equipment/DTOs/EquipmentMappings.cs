using GymGo.Domain.Equipments;

namespace GymGo.Application.Equipment.DTOs;

public static class EquipmentMappings
{
    public static EquipmentDto ToDto(this GymGo.Domain.Equipments.Equipment e) => new(
        Id:           e.Id,
        TenantId:     e.TenantId,
        Name:         e.Name,
        Brand:        e.Brand,
        Model:        e.Model,
        SerialNumber: e.SerialNumber,
        PurchaseDate: e.PurchaseDate,
        ImageUrl:     e.ImageUrl,
        IsActive:     e.IsActive,
        CreatedAtUtc: e.CreatedAtUtc,
        CreatedBy:    e.CreatedBy,
        ModifiedAtUtc: e.ModifiedAtUtc,
        ModifiedBy:   e.ModifiedBy
    );

    public static EquipmentSummaryDto ToSummaryDto(this GymGo.Domain.Equipments.Equipment e) => new(
        Id:           e.Id,
        Name:         e.Name,
        Brand:        e.Brand,
        Model:        e.Model,
        SerialNumber: e.SerialNumber,
        PurchaseDate: e.PurchaseDate,
        ImageUrl:     e.ImageUrl,
        IsActive:     e.IsActive
    );
}
