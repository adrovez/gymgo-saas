using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.WorkoutPlans.Commands.DeleteWorkoutPlan;

public sealed class DeleteWorkoutPlanCommandHandler : IRequestHandler<DeleteWorkoutPlanCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteWorkoutPlanCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(DeleteWorkoutPlanCommand request, CancellationToken cancellationToken)
    {
        var plan = await _context.WorkoutPlans
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("WorkoutPlan", request.Id);

        plan.IsDeleted    = true;
        plan.DeletedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
