using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Exceptions;
using GymGo.Domain.WorkoutLogs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.WorkoutPlans.Commands.CreateWorkoutPlan;

public sealed class CreateWorkoutPlanCommandHandler : IRequestHandler<CreateWorkoutPlanCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenant _currentTenant;

    public CreateWorkoutPlanCommandHandler(IApplicationDbContext context, ICurrentTenant currentTenant)
    {
        _context       = context;
        _currentTenant = currentTenant;
    }

    public async Task<Guid> Handle(CreateWorkoutPlanCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _currentTenant.TenantId
            ?? throw new UnauthorizedAccessException("No se pudo determinar el tenant actual.");

        var memberExists = await _context.Members
            .AnyAsync(m => m.Id == request.MemberId, cancellationToken);

        if (!memberExists)
            throw new NotFoundException("Member", request.MemberId);

        // Solo una rutina activa por socio
        var hasActivePlan = await _context.WorkoutPlans
            .AnyAsync(p =>
                p.MemberId == request.MemberId
                && p.Status == WorkoutPlanStatus.Active,
                cancellationToken);

        if (hasActivePlan)
            throw new BusinessRuleViolationException(
                "PLAN_ACTIVE_ALREADY_EXISTS",
                "El socio ya tiene una rutina activa. Completa o cancela la rutina actual antes de crear una nueva.");

        var plan = WorkoutPlan.Create(
            tenantId:                 tenantId,
            memberId:                 request.MemberId,
            objective:                request.Objective,
            startDate:                request.StartDate,
            endDate:                  request.EndDate,
            notes:                    request.Notes,
            initialWeightKg:          request.InitialWeightKg,
            initialHeightCm:          request.InitialHeightCm,
            initialBodyFatPercentage: request.InitialBodyFatPercentage);

        _context.WorkoutPlans.Add(plan);
        await _context.SaveChangesAsync(cancellationToken);

        return plan.Id;
    }
}
