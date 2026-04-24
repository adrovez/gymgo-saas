using GymGo.Domain.Members;
using MediatR;

namespace GymGo.Application.Members.Commands.ChangeMemberStatus;

/// <summary>
/// Comando para cambiar el estado operacional de un socio.
/// Usado por el staff para activar, suspender o marcar como moroso.
/// </summary>
public sealed record ChangeMemberStatusCommand(
    Guid MemberId,
    MemberStatus NewStatus
) : IRequest;
