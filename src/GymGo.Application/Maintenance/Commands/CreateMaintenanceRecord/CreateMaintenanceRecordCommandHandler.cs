using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Exceptions;
using GymGo.Domain.Maintenance;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.Maintenance.Commands.CreateMaintenanceRecord;

public sealed class CreateMaintenanceRecordCommandHandler
    : IRequestHandler<CreateMaintenanceRecordCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenant        _currentTenant;

    public CreateMaintenanceRecordCommandHandler(
        IApplicationDbContext context, ICurrentTenant currentTenant)
    {
        _context       = context;
        _currentTenant = currentTenant;
    }

    public async Task<Guid> Handle(
        CreateMaintenanceRecordCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _currentTenant.TenantId
            ?? throw new UnauthorizedAccessException("No se pudo determinar el tenant actual.");

        // Verificar que la máquina exista y pertenezca al tenant
        var equipmentExists = await _context.Equipment
            .AnyAsync(e => e.Id == request.EquipmentId, cancellationToken);

        if (!equipmentExists)
            throw new NotFoundException("Equipment", request.EquipmentId);

        var record = MaintenanceRecord.Create(
            tenantId:               tenantId,
            equipmentId:            request.EquipmentId,
            type:                   request.Type,
            scheduledDate:          request.ScheduledDate,
            description:            request.Description,
            responsibleType:        request.ResponsibleType,
            responsibleUserId:      request.ResponsibleUserId,
            externalProviderName:   request.ExternalProviderName,
            externalProviderContact: request.ExternalProviderContact);

        _context.MaintenanceRecords.Add(record);
        await _context.SaveChangesAsync(cancellationToken);

        return record.Id;
    }
}
