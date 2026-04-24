using MediatR;

namespace GymGo.Application.MembershipAssignments.Commands.UnfreezeMembership;

/// <summary>Descongela una membresía congelada, extendiendo su fecha de vencimiento.</summary>
public sealed record UnfreezeMembershipCommand(Guid AssignmentId) : IRequest;
