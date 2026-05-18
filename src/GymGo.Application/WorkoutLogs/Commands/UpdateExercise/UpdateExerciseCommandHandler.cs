using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.WorkoutLogs.Commands.UpdateExercise;

public sealed class UpdateExerciseCommandHandler : IRequestHandler<UpdateExerciseCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateExerciseCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(UpdateExerciseCommand request, CancellationToken cancellationToken)
    {
        var log = await _context.WorkoutLogs
            .Include(w => w.Exercises)
            .FirstOrDefaultAsync(w => w.Id == request.WorkoutLogId, cancellationToken)
            ?? throw new NotFoundException("WorkoutLog", request.WorkoutLogId);

        log.UpdateExercise(
            exerciseId:            request.ExerciseId,
            exerciseName:          request.ExerciseName,
            muscleGroup:           request.MuscleGroup,
            sortOrder:             request.SortOrder,
            actualSets:            request.ActualSets,
            actualReps:            request.ActualReps,
            actualWeightKg:        request.ActualWeightKg,
            actualDurationMinutes: request.ActualDurationMinutes,
            actualDistanceMeters:  request.ActualDistanceMeters,
            notes:                 request.Notes);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
