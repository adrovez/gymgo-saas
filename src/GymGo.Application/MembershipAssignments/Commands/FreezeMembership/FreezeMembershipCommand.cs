using MediatR;

namespace GymGo.Application.MembershipAssignments.Commands.FreezeMembership;

/// <summary>Congela una membresía activa (solo si el plan lo permite).</summary>
public sealed record FreezeMembershipCommand(Guid AssignmentId) : IRequest;
