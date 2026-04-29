using GymGo.Domain.Maintenance;

namespace GymGo.Application.Maintenance.DTOs;

/// <summary>Detalle completo de un registro de mantención.</summary>
public sealed record MaintenanceRecordDto(
    Guid              Id,
    Guid              TenantId,
    Guid              EquipmentId,
    string            EquipmentName,
    MaintenanceType   Type,
    string            TypeLabel,
    MaintenanceStatus Status,
    string            StatusLabel,
    DateOnly          ScheduledDate,
    DateTime?         StartedAtUtc,
    DateTime?         CompletedAtUtc,
    string            Description,
    string?           Notes,
    decimal?          Cost,
    ResponsibleType   ResponsibleType,
    string            ResponsibleTypeLabel,
    Guid?             ResponsibleUserId,
    string?           ExternalProviderName,
    string?           ExternalProviderContact,
    DateTime          CreatedAtUtc,
    string?           CreatedBy,
    DateTime?         ModifiedAtUtc,
    string?           ModifiedBy
);

/// <summary>Resumen de mantención para listados.</summary>
public sealed record MaintenanceRecordSummaryDto(
    Guid              Id,
    Guid              EquipmentId,
    string            EquipmentName,
    MaintenanceType   Type,
    string            TypeLabel,
    MaintenanceStatus Status,
    string            StatusLabel,
    DateOnly          ScheduledDate,
    string            Description,
    ResponsibleType   ResponsibleType,
    string            ResponsibleTypeLabel,
    string?           ExternalProviderName,
    decimal?          Cost
);
