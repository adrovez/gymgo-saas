using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.WorkoutPlans.Commands.UpdateWorkoutPlan;

public sealed class UpdateWorkoutPlanCommandHandler : IRequestHandler<UpdateWorkoutPlanCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateWorkoutPlanCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(UpdateWorkoutPlanCommand request, CancellationToken cancellationToken)
    {
        var plan = await _context.WorkoutPlans
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("WorkoutPlan", request.Id);

        plan.Update(
            request.Objective,
            request.StartDate,
            request.EndDate,
            request.Notes,
            request.InitialWeightKg,
            request.InitialHeightCm,
            request.InitialBodyFatPercentage);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
