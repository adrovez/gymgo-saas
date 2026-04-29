using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.Maintenance.Commands.StartMaintenance;

public sealed class StartMaintenanceCommandHandler : IRequestHandler<StartMaintenanceCommand>
{
    private readonly IApplicationDbContext _context;

    public StartMaintenanceCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(StartMaintenanceCommand request, CancellationToken cancellationToken)
    {
        var record = await _context.MaintenanceRecords
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("MaintenanceRecord", request.Id);

        record.Start();
        await _context.SaveChangesAsync(cancellationToken);
    }
}
