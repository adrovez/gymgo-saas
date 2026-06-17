using GymGo.Application.Common.Interfaces;
using GymGo.Application.MembershipAssignments.DTOs;
using GymGo.Domain.MembershipAssignments;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.MembershipAssignments.Queries.GetExpiringAssignments;

public sealed class GetExpiringAssignmentsQueryHandler
    : IRequestHandler<GetExpiringAssignmentsQuery, ExpiringAssignmentsDto>
{
    private readonly IApplicationDbContext _context;

    public GetExpiringAssignmentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ExpiringAssignmentsDto> Handle(
        GetExpiringAssignmentsQuery request, CancellationToken cancellationToken)
    {
        var today       = DateOnly.FromDateTime(DateTime.UtcNow);
        var weekLater   = today.AddDays(7);
        var twoWeeksAgo = today.AddDays(-14);

        // Una sola consulta trae ambos grupos: endDate en rango [-14 días, +7 días]
        var rows = await (
            from a in _context.MembershipAssignments.AsNoTracking()
            join m in _context.Members.AsNoTracking()
                on a.MemberId equals m.Id
            join p in _context.MembershipPlans.AsNoTracking()
                on a.MembershipPlanId equals p.Id
            where a.Status != AssignmentStatus.Cancelled
               && a.EndDate >= twoWeeksAgo
               && a.EndDate <= weekLater
            orderby a.EndDate
            select new { Assignment = a, FullName = m.FirstName + " " + m.LastName, m.Rut, PlanName = p.Name }
        ).ToListAsync(cancellationToken);

        var expiringSoon = rows
            .Where(r => r.Assignment.EndDate >= today)
            .Select(r => r.Assignment.ToSummaryDto(r.FullName, r.Rut, r.PlanName))
            .ToList();

        var recentlyExpired = rows
            .Where(r => r.Assignment.EndDate < today)
            .OrderByDescending(r => r.Assignment.EndDate)
            .Select(r => r.Assignment.ToSummaryDto(r.FullName, r.Rut, r.PlanName))
            .ToList();

        return new ExpiringAssignmentsDto(expiringSoon, recentlyExpired);
    }
}
