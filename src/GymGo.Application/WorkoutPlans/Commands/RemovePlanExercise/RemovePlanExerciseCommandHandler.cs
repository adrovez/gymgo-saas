using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.WorkoutPlans.Commands.RemovePlanExercise;

public sealed class RemovePlanExerciseCommandHandler : IRequestHandler<RemovePlanExerciseCommand>
{
    private readonly IApplicationDbContext _context;

    public RemovePlanExerciseCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(RemovePlanExerciseCommand request, CancellationToken cancellationToken)
    {
        var day = await _context.WorkoutPlanDays
            .Include(d => d.Exercises)
            .FirstOrDefaultAsync(d => d.Id == request.WorkoutPlanDayId, cancellationToken)
            ?? throw new NotFoundException("WorkoutPlanDay", request.WorkoutPlanDayId);

        day.RemoveExercise(request.ExerciseId);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
