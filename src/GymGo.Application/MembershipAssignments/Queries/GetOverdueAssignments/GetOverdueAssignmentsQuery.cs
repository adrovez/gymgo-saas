using GymGo.Application.MembershipAssignments.DTOs;
using MediatR;

namespace GymGo.Application.MembershipAssignments.Queries.GetOverdueAssignments;

/// <summary>
/// Retorna todas las asignaciones con pago moroso del tenant actual.
/// Útil para la vista de gestión de cobranza.
/// </summary>
public sealed record GetOverdueAssignmentsQuery : IRequest<IReadOnlyList<MembershipAssignmentSummaryDto>>;
