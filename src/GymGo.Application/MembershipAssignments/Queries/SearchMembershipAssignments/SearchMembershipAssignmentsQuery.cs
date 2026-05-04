using GymGo.Application.MembershipAssignments.DTOs;
using MediatR;

namespace GymGo.Application.MembershipAssignments.Queries.SearchMembershipAssignments;

/// <summary>
/// Busca asignaciones de membresía por nombre o RUT del socio.
/// Devuelve todas las asignaciones (activas, vencidas, canceladas) del socio encontrado.
/// </summary>
public sealed record SearchMembershipAssignmentsQuery(
    /// <summary>Texto a buscar: nombre completo (parcial) o RUT del socio.</summary>
    string Search
) : IRequest<IReadOnlyList<MembershipAssignmentSummaryDto>>;
