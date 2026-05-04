using GymGo.Application.Common.Interfaces;
using GymGo.Application.WorkoutLogs.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.WorkoutLogs.Queries.GetWorkoutLogs;

public sealed class GetWorkoutLogsQueryHandler
    : IRequestHandler<GetWorkoutLogsQuery, IReadOnlyList<WorkoutLogSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetWorkoutLogsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<WorkoutLogSummaryDto>> Handle(
        GetWorkoutLogsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.WorkoutLogs
            .Include(w => w.Exercises)
            .Where(w => w.MemberId == request.MemberId)
            .AsQueryable();

        if (request.From.HasValue)
            query = query.Where(w => w.Date >= request.From.Value);

        if (request.To.HasValue)
            query = query.Where(w => w.Date <= request.To.Value);

        var logs = await query
            .OrderByDescending(w => w.Date)
            .ThenByDescending(w => w.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return logs.Select(w => w.ToSummaryDto()).ToList();
    }
}
