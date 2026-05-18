using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.WorkoutPlans.Commands.AddPlanDay;

public sealed class AddPlanDayCommandHandler : IRequestHandler<AddPlanDayCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public AddPlanDayCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Guid> Handle(AddPlanDayCommand request, CancellationToken cancellationToken)
    {
        var plan = await _context.WorkoutPlans
            .Include(p => p.Days)
            .FirstOrDefaultAsync(p => p.Id == request.WorkoutPlanId, cancellationToken)
            ?? throw new NotFoundException("WorkoutPlan", request.WorkoutPlanId);

        var day = plan.AddDay(request.DayOfWeek, request.Notes);

        _context.WorkoutPlanDays.Add(day);
        await _context.SaveChangesAsync(cancellationToken);

        return day.Id;
    }
}
