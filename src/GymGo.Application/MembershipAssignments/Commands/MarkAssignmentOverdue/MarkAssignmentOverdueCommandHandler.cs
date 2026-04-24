using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.MembershipAssignments.Commands.MarkAssignmentOverdue;

public sealed class MarkAssignmentOverdueCommandHandler : IRequestHandler<MarkAssignmentOverdueCommand>
{
    private readonly IApplicationDbContext _context;

    public MarkAssignmentOverdueCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(MarkAssignmentOverdueCommand request, CancellationToken cancellationToken)
    {
        var assignment = await _context.MembershipAssignments
            .FirstOrDefaultAsync(a => a.Id == request.AssignmentId, cancellationToken)
            ?? throw new NotFoundException("MembershipAssignment", request.AssignmentId);

        assignment.MarkOverdue();

        // Marcar al socio como moroso
        var member = await _context.Members
            .FirstOrDefaultAsync(m => m.Id == assignment.MemberId, cancellationToken);

        member?.MarkAsDelinquent();

        await _context.SaveChangesAsync(cancellationToken);
    }
}
