using MediatR;

namespace GymGo.Application.MembershipAssignments.Commands.MarkAssignmentOverdue;

/// <summary>
/// Marca una asignación como morosa por falta de pago.
/// Simultáneamente cambia el estado del socio a Delinquent.
/// Típicamente invocado por un job automático al vencer el plazo de pago.
/// </summary>
public sealed record MarkAssignmentOverdueCommand(Guid AssignmentId) : IRequest;
