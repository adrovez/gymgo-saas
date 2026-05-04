using GymGo.Application.Common.Interfaces;
using GymGo.Application.MembershipAssignments.DTOs;
using GymGo.Domain.MembershipAssignments;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.MembershipAssignments.Queries.GetOverdueAssignments;

public sealed class GetOverdueAssignmentsQueryHandler
    : IRequestHandler<GetOverdueAssignmentsQuery, IReadOnlyList<MembershipAssignmentSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetOverdueAssignmentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<MembershipAssignmentSummaryDto>> Handle(
        GetOverdueAssignmentsQuery request, CancellationToken cancellationToken)
    {
        var rows = await (
            from a in _context.MembershipAssignments.AsNoTracking()
            join m in _context.Members.AsNoTracking()
                on a.MemberId equals m.Id
            join p in _context.MembershipPlans.AsNoTracking()
                on a.MembershipPlanId equals p.Id
            where a.PaymentStatus == PaymentStatus.Overdue
            orderby a.EndDate
            select new { Assignment = a, FullName = m.FirstName + " " + m.LastName, m.Rut, PlanName = p.Name }
        ).ToListAsync(cancellationToken);

        return rows
            .Select(r => r.Assignment.ToSummaryDto(r.FullName, r.Rut, r.PlanName))
            .ToList();
    }
}
