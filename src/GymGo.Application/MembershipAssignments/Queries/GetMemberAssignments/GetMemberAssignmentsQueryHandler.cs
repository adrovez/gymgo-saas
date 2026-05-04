using GymGo.Application.Common.Interfaces;
using GymGo.Application.MembershipAssignments.DTOs;
using GymGo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.MembershipAssignments.Queries.GetMemberAssignments;

public sealed class GetMemberAssignmentsQueryHandler
    : IRequestHandler<GetMemberAssignmentsQuery, IReadOnlyList<MembershipAssignmentSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetMemberAssignmentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<MembershipAssignmentSummaryDto>> Handle(
        GetMemberAssignmentsQuery request, CancellationToken cancellationToken)
    {
        // Verificar que el socio existe para proporcionar nombre/RUT en el resultado.
        var member = await _context.Members
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == request.MemberId, cancellationToken)
            ?? throw new NotFoundException("Member", request.MemberId);

        var fullName = $"{member.FirstName} {member.LastName}";

        var rows = await (
            from a in _context.MembershipAssignments.AsNoTracking()
            join p in _context.MembershipPlans.AsNoTracking()
                on a.MembershipPlanId equals p.Id
            where a.MemberId == request.MemberId
            orderby a.StartDate descending
            select new { Assignment = a, PlanName = p.Name }
        ).ToListAsync(cancellationToken);

        return rows
            .Select(r => r.Assignment.ToSummaryDto(fullName, member.Rut, r.PlanName))
            .ToList();
    }
}
