using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.MembershipAssignments.Commands.FreezeMembership;

public sealed class FreezeMembershipCommandHandler : IRequestHandler<FreezeMembershipCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeProvider _clock;

    public FreezeMembershipCommandHandler(IApplicationDbContext context, IDateTimeProvider clock)
    {
        _context = context;
        _clock = clock;
    }

    public async Task Handle(FreezeMembershipCommand request, CancellationToken cancellationToken)
    {
        var assignment = await _context.MembershipAssignments
            .FirstOrDefaultAsync(a => a.Id == request.AssignmentId, cancellationToken)
            ?? throw new NotFoundException("MembershipAssignment", request.AssignmentId);

        // Validar que el plan permite congelamiento
        var plan = await _context.MembershipPlans
            .FirstOrDefaultAsync(p => p.Id == assignment.MembershipPlanId, cancellationToken);

        if (plan is null || !plan.AllowsFreezing)
            throw new BusinessRuleViolationException(
                "ASSIGNMENT_FREEZE_NOT_ALLOWED",
                "El plan de esta membresía no permite congelamiento.");

        var today = DateOnly.FromDateTime(_clock.UtcNow);
        assignment.Freeze(today);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
