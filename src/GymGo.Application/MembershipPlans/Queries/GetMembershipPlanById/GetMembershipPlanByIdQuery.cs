using GymGo.Application.MembershipPlans.DTOs;
using MediatR;

namespace GymGo.Application.MembershipPlans.Queries.GetMembershipPlanById;

/// <summary>Obtiene el detalle completo de un plan por su Id.</summary>
public sealed record GetMembershipPlanByIdQuery(Guid PlanId) : IRequest<MembershipPlanDto>;
