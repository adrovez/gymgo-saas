using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.MembershipPlans.Commands.DeactivateMembershipPlan;

public sealed class DeactivateMembershipPlanCommandHandler : IRequestHandler<DeactivateMembershipPlanCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeProvider _clock;

    public DeactivateMembershipPlanCommandHandler(IApplicationDbContext context, IDateTimeProvider clock)
    {
        _context = context;
        _clock = clock;
    }

    public async Task Handle(DeactivateMembershipPlanCommand request, CancellationToken cancellationToken)
    {
        var plan = await _context.MembershipPlans
            .FirstOrDefaultAsync(p => p.Id == request.PlanId, cancellationToken)
            ?? throw new NotFoundException("MembershipPlan", request.PlanId);

        plan.Deactivate(_clock.UtcNow);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
