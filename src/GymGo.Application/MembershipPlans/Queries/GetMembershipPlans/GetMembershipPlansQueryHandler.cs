using GymGo.Application.Common.Interfaces;
using GymGo.Application.MembershipPlans.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.MembershipPlans.Queries.GetMembershipPlans;

public sealed class GetMembershipPlansQueryHandler
    : IRequestHandler<GetMembershipPlansQuery, IReadOnlyList<MembershipPlanSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetMembershipPlansQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<MembershipPlanSummaryDto>> Handle(
        GetMembershipPlansQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.MembershipPlans.AsNoTracking();

        if (request.IsActive.HasValue)
            query = query.Where(p => p.IsActive == request.IsActive.Value);

        if (request.Periodicity.HasValue)
            query = query.Where(p => p.Periodicity == request.Periodicity.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(search));
        }

        var entities = await query
            .OrderBy(p => p.Periodicity)
            .ThenBy(p => p.Name)
            .ToListAsync(cancellationToken);

        return entities.Select(p => p.ToSummaryDto()).ToList();
    }
}
