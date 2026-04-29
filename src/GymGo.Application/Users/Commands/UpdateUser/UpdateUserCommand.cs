using GymGo.Domain.Users;
using MediatR;

namespace GymGo.Application.Users.Commands.UpdateUser;

/// <summary>
/// Comando para actualizar los datos de un usuario existente.
/// </summary>
public sealed record UpdateUserCommand(
    Guid UserId,
    string FullName,
    UserRole Role,
    bool IsActive
) : IRequest;
