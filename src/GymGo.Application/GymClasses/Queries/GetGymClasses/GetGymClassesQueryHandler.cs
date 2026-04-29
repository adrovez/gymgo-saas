using GymGo.Application.GymClasses.DTOs;
using GymGo.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.GymClasses.Queries.GetGymClasses;

public sealed class GetGymClassesQueryHandler : IRequestHandler<GetGymClassesQuery, IReadOnlyList<GymClassSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetGymClassesQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<IReadOnlyList<GymClassSummaryDto>> Handle(
        GetGymClassesQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.GymClasses.AsQueryable();

        if (request.IsActive.HasValue)
            query = query.Where(c => c.IsActive == request.IsActive.Value);

        // Contar horarios activos por clase
        var scheduleCounts = await _context.ClassSchedules
            .Where(s => s.IsActive)
            .GroupBy(s => s.GymClassId)
            .Select(g => new { GymClassId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.GymClassId, x => x.Count, cancellationToken);

        var classes = await query
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

        return classes
            .Select(c => c.ToSummaryDto(
                scheduleCounts.TryGetValue(c.Id, out var cnt) ? cnt : 0))
            .ToList();
    }
}
