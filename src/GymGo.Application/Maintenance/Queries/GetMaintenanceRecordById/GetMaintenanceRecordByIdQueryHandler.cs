using GymGo.Application.Common.Interfaces;
using GymGo.Application.Maintenance.DTOs;
using GymGo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.Maintenance.Queries.GetMaintenanceRecordById;

public sealed class GetMaintenanceRecordByIdQueryHandler
    : IRequestHandler<GetMaintenanceRecordByIdQuery, MaintenanceRecordDto>
{
    private readonly IApplicationDbContext _context;

    public GetMaintenanceRecordByIdQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<MaintenanceRecordDto> Handle(
        GetMaintenanceRecordByIdQuery request, CancellationToken cancellationToken)
    {
        var record = await _context.MaintenanceRecords
            .Include(m => m.Equipment)
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("MaintenanceRecord", request.Id);

        return record.ToDto();
    }
}
