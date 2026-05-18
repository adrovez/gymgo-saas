using GymGo.Application.Common.Interfaces;
using GymGo.Application.WorkoutPlans.DTOs;
using GymGo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.WorkoutPlans.Queries.GetWorkoutPlanById;

public sealed class GetWorkoutPlanByIdQueryHandler : IRequestHandler<GetWorkoutPlanByIdQuery, WorkoutPlanDto>
{
    private readonly IApplicationDbContext _context;

    public GetWorkoutPlanByIdQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<WorkoutPlanDto> Handle(
        GetWorkoutPlanByIdQuery request,
        CancellationToken cancellationToken)
    {
        var plan = await _context.WorkoutPlans
            .Include(p => p.Days)
                .ThenInclude(d => d.Exercises)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("WorkoutPlan", request.Id);

        return plan.ToDto();
    }
}
