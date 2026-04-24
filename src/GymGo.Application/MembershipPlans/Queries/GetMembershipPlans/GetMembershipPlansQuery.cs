using GymGo.Application.MembershipPlans.DTOs;
using GymGo.Domain.MembershipPlans;
using MediatR;

namespace GymGo.Application.MembershipPlans.Queries.GetMembershipPlans;

/// <summary>Lista los planes del tenant actual con filtros opcionales.</summary>
public sealed record GetMembershipPlansQuery(
    string? Search = null,
    Periodicity? Periodicity = null,
    bool? IsActive = null
) : IRequest<IReadOnlyList<MembershipPlanSummaryDto>>;
