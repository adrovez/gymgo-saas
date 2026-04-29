using GymGo.Domain.Maintenance;

namespace GymGo.Application.Maintenance.DTOs;

public static class MaintenanceRecordMappings
{
    public static MaintenanceRecordDto ToDto(this MaintenanceRecord m) => new(
        Id:                     m.Id,
        TenantId:               m.TenantId,
        EquipmentId:            m.EquipmentId,
        EquipmentName:          m.Equipment?.Name ?? string.Empty,
        Type:                   m.Type,
        TypeLabel:              m.Type.ToLabel(),
        Status:                 m.Status,
        StatusLabel:            m.Status.ToLabel(),
        ScheduledDate:          m.ScheduledDate,
        StartedAtUtc:           m.StartedAtUtc,
        CompletedAtUtc:         m.CompletedAtUtc,
        Description:            m.Description,
        Notes:                  m.Notes,
        Cost:                   m.Cost,
        ResponsibleType:        m.ResponsibleType,
        ResponsibleTypeLabel:   m.ResponsibleType.ToLabel(),
        ResponsibleUserId:      m.ResponsibleUserId,
        ExternalProviderName:   m.ExternalProviderName,
        ExternalProviderContact: m.ExternalProviderContact,
        CreatedAtUtc:           m.CreatedAtUtc,
        CreatedBy:              m.CreatedBy,
        ModifiedAtUtc:          m.ModifiedAtUtc,
        ModifiedBy:             m.ModifiedBy
    );

    public static MaintenanceRecordSummaryDto ToSummaryDto(this MaintenanceRecord m) => new(
        Id:                   m.Id,
        EquipmentId:          m.EquipmentId,
        EquipmentName:        m.Equipment?.Name ?? string.Empty,
        Type:                 m.Type,
        TypeLabel:            m.Type.ToLabel(),
        Status:               m.Status,
        StatusLabel:          m.Status.ToLabel(),
        ScheduledDate:        m.ScheduledDate,
        Description:          m.Description,
        ResponsibleType:      m.ResponsibleType,
        ResponsibleTypeLabel: m.ResponsibleType.ToLabel(),
        ExternalProviderName: m.ExternalProviderName,
        Cost:                 m.Cost
    );

    // ── Helpers de etiquetas ──────────────────────────────────────────────

    public static string ToLabel(this MaintenanceType type) => type switch
    {
        MaintenanceType.Preventive => "Preventiva",
        MaintenanceType.Corrective => "Correctiva",
        _                          => type.ToString(),
    };

    public static string ToLabel(this MaintenanceStatus status) => status switch
    {
        MaintenanceStatus.Pending    => "Pendiente",
        MaintenanceStatus.InProgress => "En Proceso",
        MaintenanceStatus.Completed  => "Completada",
        MaintenanceStatus.Cancelled  => "Cancelada",
        _                            => status.ToString(),
    };

    public static string ToLabel(this ResponsibleType type) => type switch
    {
        ResponsibleType.Internal => "Interno",
        ResponsibleType.External => "Externo",
        _                        => type.ToString(),
    };
}
