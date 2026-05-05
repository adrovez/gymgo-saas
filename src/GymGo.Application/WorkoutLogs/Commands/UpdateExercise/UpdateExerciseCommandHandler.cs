using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.WorkoutLogs.Commands.UpdateExercise;

public sealed class UpdateExerciseCommandHandler : IRequestHandler<UpdateExerciseCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateExerciseCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateExerciseCommand request, CancellationToken cancellationToken)
    {
        var log = await _context.WorkoutLogs
            .Include(w => w.Exercises)
            .FirstOrDefaultAsync(w => w.Id == request.WorkoutLogId, cancellationToken)
            ?? throw new NotFoundException("WorkoutLog", request.WorkoutLogId);

        // Delegamos al aggregate para que aplique EnsureNotCompleted antes de modificar
        log.UpdateExercise(
            exerciseId:      request.ExerciseId,
            exerciseName:    request.ExerciseName,
            muscleGroup:     request.MuscleGroup,
            sortOrder:       request.SortOrder,
            sets:            request.Sets,
            reps:            request.Reps,
            weightKg:        request.WeightKg,
            durationSeconds: request.DurationSeconds,
            distanceMeters:  request.DistanceMeters,
            notes:           request.Notes);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
