using GymGo.Application.Common.Interfaces;
using GymGo.Application.WorkoutLogs.DTOs;
using GymGo.Application.WorkoutPlans.DTOs;
using GymGo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.WorkoutLogs.Queries.GetWorkoutLogById;

public sealed class GetWorkoutLogByIdQueryHandler : IRequestHandler<GetWorkoutLogByIdQuery, WorkoutLogDto>
{
    private readonly IApplicationDbContext _context;

    public GetWorkoutLogByIdQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<WorkoutLogDto> Handle(
        GetWorkoutLogByIdQuery request,
        CancellationToken cancellationToken)
    {
        var log = await _context.WorkoutLogs
            .Include(w => w.Exercises.OrderBy(e => e.SortOrder))
            .FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("WorkoutLog", request.Id);

        var day = await _context.WorkoutPlanDays
            .FirstOrDefaultAsync(d => d.Id == log.WorkoutPlanDayId, cancellationToken);

        var dayName = day is not null ? day.DayOfWeek.ToSpanish() : string.Empty;

        return log.ToDto(dayName);
    }
}
