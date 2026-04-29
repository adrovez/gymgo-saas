using MediatR;

namespace GymGo.Application.Users.Commands.ToggleUserActive;

/// <summary>
/// Comando para activar o desactivar un usuario.
/// </summary>
public sealed record ToggleUserActiveCommand(
    Guid UserId,
    bool IsActive
) : IRequest;
