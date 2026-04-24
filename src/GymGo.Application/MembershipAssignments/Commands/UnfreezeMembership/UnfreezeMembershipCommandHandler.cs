using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.MembershipAssignments.Commands.UnfreezeMembership;

public sealed class UnfreezeMembershipCommandHandler : IRequestHandler<UnfreezeMembershipCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeProvider _clock;

    public UnfreezeMembershipCommandHandler(IApplicationDbContext context, IDateTimeProvider clock)
    {
        _context = context;
        _clock = clock;
    }

    public async Task Handle(UnfreezeMembershipCommand request, CancellationToken cancellationToken)
    {
        var assignment = await _context.MembershipAssignments
            .FirstOrDefaultAsync(a => a.Id == request.AssignmentId, cancellationToken)
            ?? throw new NotFoundException("MembershipAssignment", request.AssignmentId);

        var today = DateOnly.FromDateTime(_clock.UtcNow);
        assignment.Unfreeze(today);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
