using GymGo.Application.Common.Interfaces;
using GymGo.Application.MembershipAssignments.DTOs;
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
        var entities = await _context.MembershipAssignments
            .AsNoTracking()
            .Where(a => a.MemberId == request.MemberId)
            .OrderByDescending(a => a.StartDate)
            .ToListAsync(cancellationToken);

        return entities.Select(a => a.ToSummaryDto()).ToList();
    }
}
