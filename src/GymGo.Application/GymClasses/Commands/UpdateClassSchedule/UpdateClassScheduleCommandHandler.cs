using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.GymClasses.Commands.UpdateClassSchedule;

public sealed class UpdateClassScheduleCommandHandler : IRequestHandler<UpdateClassScheduleCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateClassScheduleCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(UpdateClassScheduleCommand request, CancellationToken cancellationToken)
    {
        var schedule = await _context.ClassSchedules
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("ClassSchedule", request.Id);

        schedule.Update(
            dayOfWeek:      (DayOfWeek)request.DayOfWeek,
            startTime:      TimeOnly.Parse(request.StartTime),
            endTime:        TimeOnly.Parse(request.EndTime),
            instructorName: request.InstructorName,
            room:           request.Room,
            maxCapacity:    request.MaxCapacity);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
