using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Exceptions;
using GymGo.Domain.MembershipAssignments;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.MembershipAssignments.Commands.AssignMembershipPlan;

/// <summary>
/// Reglas de negocio aplicadas:
/// 1. El socio debe existir en el tenant actual.
/// 2. El plan debe existir, pertenecer al tenant y estar activo.
/// 3. El socio no puede tener ya una asignación Active o Frozen.
/// </summary>
public sealed class AssignMembershipPlanCommandHandler : IRequestHandler<AssignMembershipPlanCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenant _currentTenant;

    public AssignMembershipPlanCommandHandler(IApplicationDbContext context, ICurrentTenant currentTenant)
    {
        _context = context;
        _currentTenant = currentTenant;
    }

    public async Task<Guid> Handle(AssignMembershipPlanCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _currentTenant.TenantId
            ?? throw new UnauthorizedAccessException("No se pudo determinar el tenant actual.");

        // Verificar que el socio existe
        var memberExists = await _context.Members
            .AnyAsync(m => m.Id == request.MemberId, cancellationToken);

        if (!memberExists)
            throw new NotFoundException("Member", request.MemberId);

        // Verificar que el plan existe y está activo
        var plan = await _context.MembershipPlans
            .FirstOrDefaultAsync(p => p.Id == request.MembershipPlanId, cancellationToken)
            ?? throw new NotFoundException("MembershipPlan", request.MembershipPlanId);

        if (!plan.IsActive)
            throw new BusinessRuleViolationException(
                "ASSIGNMENT_PLAN_INACTIVE",
                "No se puede asignar un plan inactivo.");

        // Verificar que el socio no tiene una asignación vigente
        var hasActive = await _context.MembershipAssignments
            .AnyAsync(a =>
                a.MemberId == request.MemberId &&
                (a.Status == AssignmentStatus.Active || a.Status == AssignmentStatus.Frozen),
                cancellationToken);

        if (hasActive)
            throw new BusinessRuleViolationException(
                "ASSIGNMENT_ALREADY_ACTIVE",
                "El socio ya tiene una membresía activa o congelada. Cancélela antes de asignar una nueva.");

        var assignment = MembershipAssignment.Create(
            tenantId:          tenantId,
            memberId:          request.MemberId,
            membershipPlanId:  request.MembershipPlanId,
            planDurationDays:  plan.DurationDays,
            amountSnapshot:    plan.Amount,
            startDate:         request.StartDate,
            notes:             request.Notes);

        _context.MembershipAssignments.Add(assignment);
        await _context.SaveChangesAsync(cancellationToken);

        return assignment.Id;
    }
}
