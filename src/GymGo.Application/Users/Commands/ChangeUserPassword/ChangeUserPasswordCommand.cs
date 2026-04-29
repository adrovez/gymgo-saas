using MediatR;

namespace GymGo.Application.Users.Commands.ChangeUserPassword;

/// <summary>
/// Comando para cambiar la contraseña de un usuario.
/// </summary>
public sealed record ChangeUserPasswordCommand(
    Guid UserId,
    string NewPassword
) : IRequest;
