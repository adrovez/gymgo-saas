using GymGo.Application.Common.Interfaces;
using GymGo.Application.MembershipAssignments.DTOs;
using GymGo.Domain.MembershipAssignments;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.MembershipAssignments.Queries.GetActiveAssignment;

public sealed class GetActiveAssignmentQueryHandler
    : IRequestHandler<GetActiveAssignmentQuery, MembershipAssignmentDto?>
{
    private readonly IApplicationDbContext _context;

    public GetActiveAssignmentQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MembershipAssignmentDto?> Handle(
        GetActiveAssignmentQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.MembershipAssignments
            .AsNoTracking()
            .FirstOrDefaultAsync(a =>
                a.MemberId == request.MemberId &&
                (a.Status == AssignmentStatus.Active || a.Status == AssignmentStatus.Frozen),
                cancellationToken);

        return entity?.ToDto();
    }
}
