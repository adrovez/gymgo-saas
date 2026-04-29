using GymGo.Application.GymClasses.DTOs;
using GymGo.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.GymClasses.Queries.GetWeeklySchedule;

public sealed class GetWeeklyScheduleQueryHandler
    : IRequestHandler<GetWeeklyScheduleQuery, IReadOnlyList<ClassScheduleDto>>
{
    private readonly IApplicationDbContext _context;

    public GetWeeklyScheduleQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<IReadOnlyList<ClassScheduleDto>> Handle(
        GetWeeklyScheduleQuery request,
        CancellationToken cancellationToken)
    {
        var schedules = await _context.ClassSchedules
            .Include(s => s.GymClass)
            .Where(s => s.IsActive && s.GymClass.IsActive)
            .OrderBy(s => s.DayOfWeek)
            .ThenBy(s => s.StartTime)
            .ToListAsync(cancellationToken);

        return schedules
            .Select(s => s.ToScheduleDto(s.GymClass.Name, s.GymClass.Color))
            .ToList();
    }
}
