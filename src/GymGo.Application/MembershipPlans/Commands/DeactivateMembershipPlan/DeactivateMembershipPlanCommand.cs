using MediatR;

namespace GymGo.Application.MembershipPlans.Commands.DeactivateMembershipPlan;

/// <summary>
/// Desactiva un plan para que no pueda asignarse a nuevos socios.
/// Los socios con el plan activo no se ven afectados.
/// </summary>
public sealed record DeactivateMembershipPlanCommand(Guid PlanId) : IRequest;
