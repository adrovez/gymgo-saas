using GymGo.Application.Common.Interfaces;
using GymGo.Application.Equipment.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.Equipment.Queries.GetEquipments;

public sealed class GetEquipmentsQueryHandler
    : IRequestHandler<GetEquipmentsQuery, IReadOnlyList<EquipmentSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetEquipmentsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<IReadOnlyList<EquipmentSummaryDto>> Handle(
        GetEquipmentsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Equipment.AsQueryable();

        if (request.IsActive.HasValue)
            query = query.Where(e => e.IsActive == request.IsActive.Value);

        return await query
            .OrderBy(e => e.Name)
            .Select(e => e.ToSummaryDto())
            .ToListAsync(cancellationToken);
    }
}
