using GymGo.Application.MembershipAssignments.DTOs;
using MediatR;

namespace GymGo.Application.MembershipAssignments.Queries.GetExpiringAssignments;

/// <summary>
/// Retorna membresías por vencer (próximos 7 días) y vencidas recientemente (últimos 14 días).
/// Filtra por EndDate, no por Status, porque el status no se actualiza automáticamente.
/// Excluye membresías canceladas.
/// </summary>
public sealed record GetExpiringAssignmentsQuery : IRequest<ExpiringAssignmentsDto>;
