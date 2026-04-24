using MediatR;

namespace GymGo.Application.MembershipAssignments.Commands.CancelAssignment;

/// <summary>Cancela manualmente una asignación de membresía activa.</summary>
public sealed record CancelAssignmentCommand(Guid AssignmentId) : IRequest;
