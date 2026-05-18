using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.WorkoutPlans.Commands.AddPlanExercise;

public sealed class AddPlanExerciseCommandHandler : IRequestHandler<AddPlanExerciseCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public AddPlanExerciseCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Guid> Handle(AddPlanExerciseCommand request, CancellationToken cancellationToken)
    {
        var day = await _context.WorkoutPlanDays
            .Include(d => d.Exercises)
            .FirstOrDefaultAsync(d => d.Id == request.WorkoutPlanDayId, cancellationToken)
            ?? throw new NotFoundException("WorkoutPlanDay", request.WorkoutPlanDayId);

        var exercise = day.AddExercise(
            exerciseName:           request.ExerciseName,
            muscleGroup:            request.MuscleGroup,
            plannedSets:            request.PlannedSets,
            plannedReps:            request.PlannedReps,
            plannedWeightKg:        request.PlannedWeightKg,
            plannedDurationMinutes: request.PlannedDurationMinutes,
            plannedDistanceMeters:  request.PlannedDistanceMeters,
            notes:                  request.Notes);

        _context.WorkoutPlanExercises.Add(exercise);
        await _context.SaveChangesAsync(cancellationToken);

        return exercise.Id;
    }
}
