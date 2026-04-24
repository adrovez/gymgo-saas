using GymGo.Application.Common.Interfaces;
using GymGo.Domain.MembershipPlans;
using MediatR;

namespace GymGo.Application.MembershipPlans.Commands.CreateMembershipPlan;

public sealed class CreateMembershipPlanCommandHandler : IRequestHandler<CreateMembershipPlanCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenant _currentTenant;

    public CreateMembershipPlanCommandHandler(IApplicationDbContext context, ICurrentTenant currentTenant)
    {
        _context = context;
        _currentTenant = currentTenant;
    }

    public async Task<Guid> Handle(CreateMembershipPlanCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _currentTenant.TenantId
            ?? throw new UnauthorizedAccessException("No se pudo determinar el tenant actual.");

        var plan = MembershipPlan.Create(
            tenantId:       tenantId,
            name:           request.Name,
            description:    request.Description,
            periodicity:    request.Periodicity,
            daysPerWeek:    request.DaysPerWeek,
            fixedDays:      request.FixedDays,
            monday:         request.Monday,
            tuesday:        request.Tuesday,
            wednesday:      request.Wednesday,
            thursday:       request.Thursday,
            friday:         request.Friday,
            saturday:       request.Saturday,
            sunday:         request.Sunday,
            freeSchedule:   request.FreeSchedule,
            timeFrom:       request.TimeFrom,
            timeTo:         request.TimeTo,
            amount:         request.Amount,
            allowsFreezing: request.AllowsFreezing);

        _context.MembershipPlans.Add(plan);
        await _context.SaveChangesAsync(cancellationToken);

        return plan.Id;
    }
}
