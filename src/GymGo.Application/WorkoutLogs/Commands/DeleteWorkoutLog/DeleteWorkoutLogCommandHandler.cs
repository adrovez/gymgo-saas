using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.WorkoutLogs.Commands.DeleteWorkoutLog;

public sealed class DeleteWorkoutLogCommandHandler : IRequestHandler<DeleteWorkoutLogCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public DeleteWorkoutLogCommandHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context     = context;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteWorkoutLogCommand request, CancellationToken cancellationToken)
    {
        var log = await _context.WorkoutLogs
            .FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("WorkoutLog", request.Id);

        // Soft delete manual (el interceptor de auditoría maneja CreatedBy/ModifiedBy;
        // el DeletedBy lo asignamos aquí para tener trazabilidad de quién eliminó).
        log.IsDeleted    = true;
        log.DeletedAtUtc = DateTime.UtcNow;
        log.DeletedBy    = _currentUser.UserId?.ToString();

        await _context.SaveChangesAsync(cancellationToken);
    }
}
