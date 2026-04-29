using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.Equipment.Commands.DeactivateEquipment;

public sealed class DeactivateEquipmentCommandHandler : IRequestHandler<DeactivateEquipmentCommand>
{
    private readonly IApplicationDbContext _context;

    public DeactivateEquipmentCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(DeactivateEquipmentCommand request, CancellationToken cancellationToken)
    {
        var equipment = await _context.Equipment
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Equipment", request.Id);

        equipment.Deactivate();
        await _context.SaveChangesAsync(cancellationToken);
    }
}
