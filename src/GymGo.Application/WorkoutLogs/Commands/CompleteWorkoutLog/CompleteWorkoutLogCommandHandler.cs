using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.WorkoutLogs.Commands.CompleteWorkoutLog;

public sealed class CompleteWorkoutLogCommandHandler : IRequestHandler<CompleteWorkoutLogCommand>
{
    private readonly IApplicationDbContext _context;

    public CompleteWorkoutLogCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(CompleteWorkoutLogCommand request, CancellationToken cancellationToken)
    {
        // Incluir ejercicios para que Complete() pueda verificar que hay al menos uno
        var log = await _context.WorkoutLogs
            .Include(w => w.Exercises)
            .FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("WorkoutLog", request.Id);

        log.Complete();

        await _context.SaveChangesAsync(cancellationToken);
    }
}
