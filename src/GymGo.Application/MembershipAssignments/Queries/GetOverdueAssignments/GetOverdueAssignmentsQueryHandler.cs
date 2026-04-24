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
        var entities = await _context.MembershipAssignments
            .AsNoTracking()
            .Where(a => a.PaymentStatus == PaymentStatus.Overdue)
            .OrderBy(a => a.EndDate)
            .ToListAsync(cancellationToken);

        return entities.Select(a => a.ToSummaryDto()).ToList();
    }
}
