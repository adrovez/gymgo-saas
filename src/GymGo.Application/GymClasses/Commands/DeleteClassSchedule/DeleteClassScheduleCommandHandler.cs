using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.GymClasses.Commands.DeleteClassSchedule;

public sealed class DeleteClassScheduleCommandHandler : IRequestHandler<DeleteClassScheduleCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteClassScheduleCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(DeleteClassScheduleCommand request, CancellationToken cancellationToken)
    {
        var schedule = await _context.ClassSchedules
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("ClassSchedule", request.Id);

        // Soft delete a través del interceptor
        _context.ClassSchedules.Remove(schedule);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
