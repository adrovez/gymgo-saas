using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.MembershipPlans.Commands.ReactivateMembershipPlan;

public sealed class ReactivateMembershipPlanCommandHandler : IRequestHandler<ReactivateMembershipPlanCommand>
{
    private readonly IApplicationDbContext _context;

    public ReactivateMembershipPlanCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ReactivateMembershipPlanCommand request, CancellationToken cancellationToken)
    {
        var plan = await _context.MembershipPlans
            .FirstOrDefaultAsync(p => p.Id == request.PlanId, cancellationToken)
            ?? throw new NotFoundException("MembershipPlan", request.PlanId);

        plan.Reactivate();
        await _context.SaveChangesAsync(cancellationToken);
    }
}
