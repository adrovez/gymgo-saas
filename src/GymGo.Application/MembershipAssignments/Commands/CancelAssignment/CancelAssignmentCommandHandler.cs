using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.MembershipAssignments.Commands.CancelAssignment;

public sealed class CancelAssignmentCommandHandler : IRequestHandler<CancelAssignmentCommand>
{
    private readonly IApplicationDbContext _context;

    public CancelAssignmentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(CancelAssignmentCommand request, CancellationToken cancellationToken)
    {
        var assignment = await _context.MembershipAssignments
            .FirstOrDefaultAsync(a => a.Id == request.AssignmentId, cancellationToken)
            ?? throw new NotFoundException("MembershipAssignment", request.AssignmentId);

        assignment.Cancel();
        await _context.SaveChangesAsync(cancellationToken);
    }
}
