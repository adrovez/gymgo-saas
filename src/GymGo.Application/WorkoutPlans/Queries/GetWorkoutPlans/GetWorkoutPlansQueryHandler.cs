using GymGo.Application.Common.Interfaces;
using GymGo.Application.WorkoutPlans.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.WorkoutPlans.Queries.GetWorkoutPlans;

public sealed class GetWorkoutPlansQueryHandler
    : IRequestHandler<GetWorkoutPlansQuery, IReadOnlyList<WorkoutPlanSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetWorkoutPlansQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<IReadOnlyList<WorkoutPlanSummaryDto>> Handle(
        GetWorkoutPlansQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.WorkoutPlans
            .Include(p => p.Days)
            .Where(p => p.MemberId == request.MemberId)
            .AsQueryable();

        if (request.Status.HasValue)
            query = query.Where(p => p.Status == request.Status.Value);

        var plans = await query
            .OrderByDescending(p => p.StartDate)
            .ToListAsync(cancellationToken);

        return plans.Select(p => p.ToSummaryDto()).ToList();
    }
}
