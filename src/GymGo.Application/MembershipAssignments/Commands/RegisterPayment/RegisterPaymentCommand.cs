using MediatR;

namespace GymGo.Application.MembershipAssignments.Commands.RegisterPayment;

/// <summary>
/// Registra el pago de una asignación de membresía.
/// Si el socio estaba Moroso, lo reactiva a Activo automáticamente.
/// </summary>
public sealed record RegisterPaymentCommand(Guid AssignmentId) : IRequest;
