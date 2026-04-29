using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Exceptions;
using GymGo.Domain.GymClasses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.GymClasses.Commands.CreateClassSchedule;

public sealed class CreateClassScheduleCommandHandler : IRequestHandler<CreateClassScheduleCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenant _currentTenant;

    public CreateClassScheduleCommandHandler(IApplicationDbContext context, ICurrentTenant currentTenant)
    {
        _context       = context;
        _currentTenant = currentTenant;
    }

    public async Task<Guid> Handle(CreateClassScheduleCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _currentTenant.TenantId
            ?? throw new UnauthorizedAccessException("No se pudo determinar el tenant actual.");

        var gymClass = await _context.GymClasses
            .FirstOrDefaultAsync(c => c.Id == request.GymClassId, cancellationToken)
            ?? throw new NotFoundException("GymClass", request.GymClassId);

        if (!gymClass.IsActive)
            throw new BusinessRuleViolationException(
                "SCHEDULE_CLASS_INACTIVE",
                "No se puede crear un horario para una clase inactiva.");

        var startTime = TimeOnly.Parse(request.StartTime);
        var endTime   = TimeOnly.Parse(request.EndTime);

        var schedule = ClassSchedule.Create(
            tenantId:       tenantId,
            gymClassId:     request.GymClassId,
            dayOfWeek:      (DayOfWeek)request.DayOfWeek,
            startTime:      startTime,
            endTime:        endTime,
            instructorName: request.InstructorName,
            room:           request.Room,
            maxCapacity:    request.MaxCapacity);

        _context.ClassSchedules.Add(schedule);
        await _context.SaveChangesAsync(cancellationToken);

        return schedule.Id;
    }
}
