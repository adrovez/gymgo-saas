using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.WorkoutLogs.Commands.RemoveExercise;

public sealed class RemoveExerciseCommandHandler : IRequestHandler<RemoveExerciseCommand>
{
    private readonly IApplicationDbContext _context;

    public RemoveExerciseCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(RemoveExerciseCommand request, CancellationToken cancellationToken)
    {
        var log = await _context.WorkoutLogs
            .Include(w => w.Exercises)
            .FirstOrDefaultAsync(w => w.Id == request.WorkoutLogId, cancellationToken)
            ?? throw new NotFoundException("WorkoutLog", request.WorkoutLogId);

        log.RemoveExercise(request.ExerciseId);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
