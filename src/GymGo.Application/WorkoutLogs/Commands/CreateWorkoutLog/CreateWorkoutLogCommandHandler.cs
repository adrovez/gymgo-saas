using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Exceptions;
using GymGo.Domain.WorkoutLogs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.WorkoutLogs.Commands.CreateWorkoutLog;

public sealed class CreateWorkoutLogCommandHandler : IRequestHandler<CreateWorkoutLogCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenant _currentTenant;

    public CreateWorkoutLogCommandHandler(IApplicationDbContext context, ICurrentTenant currentTenant)
    {
        _context       = context;
        _currentTenant = currentTenant;
    }

    public async Task<Guid> Handle(CreateWorkoutLogCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _currentTenant.TenantId
            ?? throw new UnauthorizedAccessException("No se pudo determinar el tenant actual.");

        // Verificar que el plan existe, está activo y pertenece al socio
        var plan = await _context.WorkoutPlans
            .FirstOrDefaultAsync(p =>
                p.Id       == request.WorkoutPlanId
                && p.MemberId == request.MemberId
                && p.Status   == WorkoutPlanStatus.Active,
                cancellationToken)
            ?? throw new NotFoundException("WorkoutPlan", request.WorkoutPlanId);

        // Verificar que el día pertenece al plan
        var dayExists = await _context.WorkoutPlanDays
            .AnyAsync(d =>
                d.Id            == request.WorkoutPlanDayId
                && d.WorkoutPlanId == request.WorkoutPlanId,
                cancellationToken);

        if (!dayExists)
            throw new NotFoundException("WorkoutPlanDay", request.WorkoutPlanDayId);

        var log = WorkoutLog.Create(
            tenantId:         tenantId,
            memberId:         request.MemberId,
            workoutPlanId:    request.WorkoutPlanId,
            workoutPlanDayId: request.WorkoutPlanDayId,
            date:             request.Date,
            notes:            request.Notes);

        _context.WorkoutLogs.Add(log);
        await _context.SaveChangesAsync(cancellationToken);

        return log.Id;
    }
}
