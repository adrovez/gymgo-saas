using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.WorkoutLogs.Commands.AddExercise;

public sealed class AddExerciseCommandHandler : IRequestHandler<AddExerciseCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public AddExerciseCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Guid> Handle(AddExerciseCommand request, CancellationToken cancellationToken)
    {
        var log = await _context.WorkoutLogs
            .Include(w => w.Exercises)
            .FirstOrDefaultAsync(w => w.Id == request.WorkoutLogId, cancellationToken)
            ?? throw new NotFoundException("WorkoutLog", request.WorkoutLogId);

        var exercise = log.AddExercise(
            exerciseName:          request.ExerciseName,
            muscleGroup:           request.MuscleGroup,
            workoutPlanExerciseId: request.WorkoutPlanExerciseId,
            isExtra:               request.IsExtra,
            actualSets:            request.ActualSets,
            actualReps:            request.ActualReps,
            actualWeightKg:        request.ActualWeightKg,
            actualDurationMinutes: request.ActualDurationMinutes,
            actualDistanceMeters:  request.ActualDistanceMeters,
            notes:                 request.Notes);

        _context.WorkoutLogExercises.Add(exercise);
        await _context.SaveChangesAsync(cancellationToken);

        return exercise.Id;
    }
}
