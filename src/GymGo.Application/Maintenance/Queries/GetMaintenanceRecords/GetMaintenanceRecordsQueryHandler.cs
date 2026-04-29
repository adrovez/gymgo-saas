using GymGo.Application.Common.Interfaces;
using GymGo.Application.Maintenance.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.Maintenance.Queries.GetMaintenanceRecords;

public sealed class GetMaintenanceRecordsQueryHandler
    : IRequestHandler<GetMaintenanceRecordsQuery, IReadOnlyList<MaintenanceRecordSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetMaintenanceRecordsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<IReadOnlyList<MaintenanceRecordSummaryDto>> Handle(
        GetMaintenanceRecordsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.MaintenanceRecords
            .Include(m => m.Equipment)
            .AsQueryable();

        if (request.EquipmentId.HasValue)
            query = query.Where(m => m.EquipmentId == request.EquipmentId.Value);

        if (request.Type.HasValue)
            query = query.Where(m => m.Type == request.Type.Value);

        if (request.Status.HasValue)
            query = query.Where(m => m.Status == request.Status.Value);

        return await query
            .OrderByDescending(m => m.ScheduledDate)
            .ThenBy(m => m.Equipment.Name)
            .Select(m => m.ToSummaryDto())
            .ToListAsync(cancellationToken);
    }
}
