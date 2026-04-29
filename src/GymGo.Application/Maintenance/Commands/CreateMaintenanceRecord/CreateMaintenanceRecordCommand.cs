using GymGo.Domain.Maintenance;
using MediatR;

namespace GymGo.Application.Maintenance.Commands.CreateMaintenanceRecord;

public sealed record CreateMaintenanceRecordCommand(
    Guid            EquipmentId,
    MaintenanceType Type,
    DateOnly        ScheduledDate,
    string          Description,
    ResponsibleType ResponsibleType,
    Guid?           ResponsibleUserId,
    string?         ExternalProviderName,
    string?         ExternalProviderContact
) : IRequest<Guid>;
