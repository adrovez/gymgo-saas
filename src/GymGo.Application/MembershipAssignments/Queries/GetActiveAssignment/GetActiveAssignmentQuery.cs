using GymGo.Application.MembershipAssignments.DTOs;
using MediatR;

namespace GymGo.Application.MembershipAssignments.Queries.GetActiveAssignment;

/// <summary>
/// Retorna la membresía activa o congelada de un socio.
/// Retorna null si el socio no tiene membresía vigente.
/// </summary>
public sealed record GetActiveAssignmentQuery(Guid MemberId) : IRequest<MembershipAssignmentDto?>;
