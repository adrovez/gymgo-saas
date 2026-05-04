using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.WorkoutLogs.Commands.UpdateWorkoutLog;

public sealed class UpdateWorkoutLogCommandHandler : IRequestHandler<UpdateWorkoutLogCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateWorkoutLogCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateWorkoutLogCommand request, CancellationToken cancellationToken)
    {
        var log = await _context.WorkoutLogs
            .FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("WorkoutLog", request.Id);

        log.Update(request.Title, request.Notes);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
