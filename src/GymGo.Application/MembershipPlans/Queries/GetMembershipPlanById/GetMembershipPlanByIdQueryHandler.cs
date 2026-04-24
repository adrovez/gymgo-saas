using GymGo.Application.Common.Interfaces;
using GymGo.Application.MembershipPlans.DTOs;
using GymGo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.MembershipPlans.Queries.GetMembershipPlanById;

public sealed class GetMembershipPlanByIdQueryHandler
    : IRequestHandler<GetMembershipPlanByIdQuery, MembershipPlanDto>
{
    private readonly IApplicationDbContext _context;

    public GetMembershipPlanByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MembershipPlanDto> Handle(
        GetMembershipPlanByIdQuery request,
        CancellationToken cancellationToken)
    {
        var plan = await _context.MembershipPlans
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.PlanId, cancellationToken)
            ?? throw new NotFoundException("MembershipPlan", request.PlanId);

        return plan.ToDto();
    }
}
