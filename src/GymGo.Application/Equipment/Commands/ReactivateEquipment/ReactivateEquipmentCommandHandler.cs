using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.Equipment.Commands.ReactivateEquipment;

public sealed class ReactivateEquipmentCommandHandler : IRequestHandler<ReactivateEquipmentCommand>
{
    private readonly IApplicationDbContext _context;

    public ReactivateEquipmentCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(ReactivateEquipmentCommand request, CancellationToken cancellationToken)
    {
        var equipment = await _context.Equipment
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Equipment", request.Id);

        equipment.Reactivate();
        await _context.SaveChangesAsync(cancellationToken);
    }
}
