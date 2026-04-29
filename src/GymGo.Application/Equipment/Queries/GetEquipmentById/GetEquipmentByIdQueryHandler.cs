using GymGo.Application.Common.Interfaces;
using GymGo.Application.Equipment.DTOs;
using GymGo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.Equipment.Queries.GetEquipmentById;

public sealed class GetEquipmentByIdQueryHandler : IRequestHandler<GetEquipmentByIdQuery, EquipmentDto>
{
    private readonly IApplicationDbContext _context;

    public GetEquipmentByIdQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<EquipmentDto> Handle(
        GetEquipmentByIdQuery request, CancellationToken cancellationToken)
    {
        var equipment = await _context.Equipment
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Equipment", request.Id);

        return equipment.ToDto();
    }
}
