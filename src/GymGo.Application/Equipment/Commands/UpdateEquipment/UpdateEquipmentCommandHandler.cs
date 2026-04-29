using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.Equipment.Commands.UpdateEquipment;

public sealed class UpdateEquipmentCommandHandler : IRequestHandler<UpdateEquipmentCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateEquipmentCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(UpdateEquipmentCommand request, CancellationToken cancellationToken)
    {
        var equipment = await _context.Equipment
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Equipment", request.Id);

        equipment.Update(
            name:         request.Name,
            brand:        request.Brand,
            model:        request.Model,
            serialNumber: request.SerialNumber,
            purchaseDate: request.PurchaseDate,
            imageUrl:     request.ImageUrl);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
