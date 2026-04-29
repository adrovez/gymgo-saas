using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.Maintenance.Commands.CompleteMaintenance;

public sealed class CompleteMaintenanceCommandHandler : IRequestHandler<CompleteMaintenanceCommand>
{
    private readonly IApplicationDbContext _context;

    public CompleteMaintenanceCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(CompleteMaintenanceCommand request, CancellationToken cancellationToken)
    {
        var record = await _context.MaintenanceRecords
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("MaintenanceRecord", request.Id);

        record.Complete(request.Notes, request.Cost);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
