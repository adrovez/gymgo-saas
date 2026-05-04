using GymGo.Application.Common.Interfaces;
using GymGo.Application.MembershipAssignments.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.MembershipAssignments.Queries.SearchMembershipAssignments;

public sealed class SearchMembershipAssignmentsQueryHandler
    : IRequestHandler<SearchMembershipAssignmentsQuery, IReadOnlyList<MembershipAssignmentSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public SearchMembershipAssignmentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<MembershipAssignmentSummaryDto>> Handle(
        SearchMembershipAssignmentsQuery request, CancellationToken cancellationToken)
    {
        // Retornar vacío si la búsqueda es demasiado corta (validación también en frontend)
        if (string.IsNullOrWhiteSpace(request.Search) || request.Search.Trim().Length < 2)
            return Array.Empty<MembershipAssignmentSummaryDto>();

        var search = request.Search.Trim();

        var rows = await (
            from a in _context.MembershipAssignments.AsNoTracking()
            join m in _context.Members.AsNoTracking()
                on a.MemberId equals m.Id
            join p in _context.MembershipPlans.AsNoTracking()
                on a.MembershipPlanId equals p.Id
            where EF.Functions.Like(m.FirstName + " " + m.LastName, $"%{search}%")
               || EF.Functions.Like(m.Rut, $"%{search}%")
            orderby m.LastName, m.FirstName, a.StartDate descending
            select new { Assignment = a, FullName = m.FirstName + " " + m.LastName, m.Rut, PlanName = p.Name }
        ).ToListAsync(cancellationToken);

        return rows
            .Select(r => r.Assignment.ToSummaryDto(r.FullName, r.Rut, r.PlanName))
            .ToList();
    }
}
