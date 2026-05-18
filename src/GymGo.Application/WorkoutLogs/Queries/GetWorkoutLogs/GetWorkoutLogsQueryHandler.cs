using GymGo.Application.Common.Interfaces;
using GymGo.Application.WorkoutLogs.DTOs;
using GymGo.Application.WorkoutPlans.DTOs;
using GymGo.Domain.WorkoutLogs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.WorkoutLogs.Queries.GetWorkoutLogs;

public sealed class GetWorkoutLogsQueryHandler
    : IRequestHandler<GetWorkoutLogsQuery, IReadOnlyList<WorkoutLogSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetWorkoutLogsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<IReadOnlyList<WorkoutLogSummaryDto>> Handle(
        GetWorkoutLogsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.WorkoutLogs
            .Include(w => w.Exercises)
            .Where(w => w.MemberId == request.MemberId)
            .AsQueryable();

        if (request.WorkoutPlanId.HasValue)
            query = query.Where(w => w.WorkoutPlanId == request.WorkoutPlanId.Value);

        if (request.From.HasValue)
            query = query.Where(w => w.Date >= request.From.Value);

        if (request.To.HasValue)
            query = query.Where(w => w.Date <= request.To.Value);

        var logs = await query
            .OrderByDescending(w => w.Date)
            .ThenByDescending(w => w.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        // Cargar días para obtener el nombre del día de semana
        var dayIds = logs.Select(w => w.WorkoutPlanDayId).Distinct().ToList();
        var days   = await _context.WorkoutPlanDays
            .Where(d => dayIds.Contains(d.Id))
            .ToListAsync(cancellationToken);

        var dayMap = days.ToDictionary(d => d.Id, d => d.DayOfWeek.ToSpanish());

        return logs
            .Select(w => w.ToSummaryDto(
                dayMap.TryGetValue(w.WorkoutPlanDayId, out var name) ? name : string.Empty))
            .ToList();
    }
}
