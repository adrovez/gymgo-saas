using GymGo.Application.Auth.DTOs;
using MediatR;

namespace GymGo.Application.Auth.Commands.Login;

/// <summary>
/// Comando de autenticación. Valida credenciales y retorna un JWT.
/// El tenant se resuelve desde el header X-Tenant-Id (via ICurrentTenant).
/// PlatformAdmin puede autenticarse sin header de tenant.
/// </summary>
public sealed record LoginCommand(
    string Email,
    string Password
) : IRequest<LoginResponseDto>;
