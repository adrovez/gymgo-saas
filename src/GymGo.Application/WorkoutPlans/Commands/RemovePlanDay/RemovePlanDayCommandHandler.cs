using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.WorkoutPlans.Commands.RemovePlanDay;

public sealed class RemovePlanDayCommandHandler : IRequestHandler<RemovePlanDayCommand>
{
    private readonly IApplicationDbContext _context;

    public RemovePlanDayCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(RemovePlanDayCommand request, CancellationToken cancellationToken)
    {
        var plan = await _context.WorkoutPlans
            .Include(p => p.Days)
            .FirstOrDefaultAsync(p => p.Id == request.WorkoutPlanId, cancellationToken)
            ?? throw new NotFoundException("WorkoutPlan", request.WorkoutPlanId);

        plan.RemoveDay(request.DayId);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
