using MediatR;

namespace GymGo.Application.MembershipAssignments.Commands.AssignMembershipPlan;

/// <summary>
/// Asigna un plan de membresía a un socio.
/// Retorna el Id de la nueva asignación.
/// </summary>
public sealed record AssignMembershipPlanCommand(
    Guid MemberId,
    Guid MembershipPlanId,
    DateOnly? StartDate,
    string? Notes
) : IRequest<Guid>;
