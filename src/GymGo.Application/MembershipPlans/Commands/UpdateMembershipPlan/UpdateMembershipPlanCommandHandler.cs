using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.MembershipPlans.Commands.UpdateMembershipPlan;

public sealed class UpdateMembershipPlanCommandHandler : IRequestHandler<UpdateMembershipPlanCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateMembershipPlanCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateMembershipPlanCommand request, CancellationToken cancellationToken)
    {
        var plan = await _context.MembershipPlans
            .FirstOrDefaultAsync(p => p.Id == request.PlanId, cancellationToken)
            ?? throw new NotFoundException("MembershipPlan", request.PlanId);

        plan.Update(
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

        await _context.SaveChangesAsync(cancellationToken);
    }
}
