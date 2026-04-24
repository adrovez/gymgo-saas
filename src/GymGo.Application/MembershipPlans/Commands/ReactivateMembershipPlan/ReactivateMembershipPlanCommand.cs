using MediatR;

namespace GymGo.Application.MembershipPlans.Commands.ReactivateMembershipPlan;

/// <summary>Reactiva un plan previamente desactivado.</summary>
public sealed record ReactivateMembershipPlanCommand(Guid PlanId) : IRequest;
