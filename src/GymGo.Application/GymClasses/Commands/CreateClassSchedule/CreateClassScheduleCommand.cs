using MediatR;

namespace GymGo.Application.GymClasses.Commands.CreateClassSchedule;

public sealed record CreateClassScheduleCommand(
    Guid GymClassId,
    int DayOfWeek,
    string StartTime,   // "HH:mm"
    string EndTime,     // "HH:mm"
    string? InstructorName,
    string? Room,
    int? MaxCapacity
) : IRequest<Guid>;
