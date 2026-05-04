using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.WorkoutLogs.Commands.AddExercise;

public sealed class AddExerciseCommandHandler : IRequestHandler<AddExerciseCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public AddExerciseCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(AddExerciseCommand request, CancellationToken cancellationToken)
    {
        var log = await _context.WorkoutLogs
            .Include(w => w.Exercises)
            .FirstOrDefaultAsync(w => w.Id == request.WorkoutLogId, cancellationToken)
            ?? throw new NotFoundException("WorkoutLog", request.WorkoutLogId);

        var exercise = log.AddExercise(
            exerciseName:    request.ExerciseName,
            muscleGroup:     request.MuscleGroup,
            sets:            request.Sets,
            reps:            request.Reps,
            weightKg:        request.WeightKg,
            durationSeconds: request.DurationSeconds,
            distanceMeters:  request.DistanceMeters,
            notes:           request.Notes);

        await _context.SaveChangesAsync(cancellationToken);

        return exercise.Id;
    }
}
