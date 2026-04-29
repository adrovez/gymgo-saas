using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Equipments;
using MediatR;

namespace GymGo.Application.Equipment.Commands.CreateEquipment;

public sealed class CreateEquipmentCommandHandler : IRequestHandler<CreateEquipmentCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenant        _currentTenant;

    public CreateEquipmentCommandHandler(IApplicationDbContext context, ICurrentTenant currentTenant)
    {
        _context       = context;
        _currentTenant = currentTenant;
    }

    public async Task<Guid> Handle(CreateEquipmentCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _currentTenant.TenantId
            ?? throw new UnauthorizedAccessException("No se pudo determinar el tenant actual.");

        var equipment = GymGo.Domain.Equipments.Equipment.Create(
            tenantId:     tenantId,
            name:         request.Name,
            brand:        request.Brand,
            model:        request.Model,
            serialNumber: request.SerialNumber,
            purchaseDate: request.PurchaseDate,
            imageUrl:     request.ImageUrl);

        _context.Equipment.Add(equipment);
        await _context.SaveChangesAsync(cancellationToken);

        return equipment.Id;
    }
}
