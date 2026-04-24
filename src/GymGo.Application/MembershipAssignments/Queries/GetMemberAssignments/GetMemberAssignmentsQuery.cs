using GymGo.Application.MembershipAssignments.DTOs;
using MediatR;

namespace GymGo.Application.MembershipAssignments.Queries.GetMemberAssignments;

/// <summary>Retorna el historial completo de membresías de un socio, ordenado de más reciente a más antiguo.</summary>
public sealed record GetMemberAssignmentsQuery(Guid MemberId) : IRequest<IReadOnlyList<MembershipAssignmentSummaryDto>>;
