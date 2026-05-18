using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.WorkoutPlans.Commands.UpdatePlanExercise;

public sealed class UpdatePlanExerciseCommandHandler : IRequestHandler<UpdatePlanExerciseCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdatePlanExerciseCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(UpdatePlanExerciseCommand request, CancellationToken cancellationToken)
    {
        var day = await _context.WorkoutPlanDays
            .Include(d => d.Exercises)
            .FirstOrDefaultAsync(d => d.Id == request.WorkoutPlanDayId, cancellationToken)
            ?? throw new NotFoundException("WorkoutPlanDay", request.WorkoutPlanDayId);

        day.UpdateExercise(
            request.ExerciseId,
            request.ExerciseName,
            request.MuscleGroup,
            request.SortOrder,
            request.PlannedSets,
            request.PlannedReps,
            request.PlannedWeightKg,
            request.PlannedDurationMinutes,
            request.PlannedDistanceMeters,
            request.Notes);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
