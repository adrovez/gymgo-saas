using GymGo.Domain.Users;
using MediatR;

namespace GymGo.Application.Users.Commands.CreateUser;

/// <summary>
/// Comando para crear un nuevo usuario en el tenant actual.
/// Solo se permiten roles GymStaff e Instructor desde la UI.
/// El TenantId lo inyecta el handler desde ICurrentTenant.
/// </summary>
public sealed record CreateUserCommand(
    string FullName,
    string Email,
    string Password,
    UserRole Role
) : IRequest<Guid>;
