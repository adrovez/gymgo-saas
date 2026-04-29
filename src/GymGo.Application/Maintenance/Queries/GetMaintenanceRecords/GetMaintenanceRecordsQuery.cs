using GymGo.Application.Maintenance.DTOs;
using GymGo.Domain.Maintenance;
using MediatR;

namespace GymGo.Application.Maintenance.Queries.GetMaintenanceRecords;

/// <summary>
/// Devuelve los registros de mantención del tenant.
/// Todos los filtros son opcionales y se pueden combinar.
/// </summary>
public sealed record GetMaintenanceRecordsQuery(
    Guid?             EquipmentId,
    MaintenanceType?  Type,
    MaintenanceStatus? Status
) : IRequest<IReadOnlyList<MaintenanceRecordSummaryDto>>;
