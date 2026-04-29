using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.Maintenance.Commands.CancelMaintenance;

public sealed class CancelMaintenanceCommandHandler : IRequestHandler<CancelMaintenanceCommand>
{
    private readonly IApplicationDbContext _context;

    public CancelMaintenanceCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(CancelMaintenanceCommand request, CancellationToken cancellationToken)
    {
        var record = await _context.MaintenanceRecords
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("MaintenanceRecord", request.Id);

        record.Cancel(request.Reason);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
