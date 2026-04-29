using MediatR;

namespace GymGo.Application.GymClasses.Commands.UpdateClassSchedule;

public sealed record UpdateClassScheduleCommand(
    Guid Id,
    int DayOfWeek,
    string StartTime,
    string EndTime,
    string? InstructorName,
    string? Room,
    int? MaxCapacity
) : IRequest;
