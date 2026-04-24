using GymGo.Domain.Users;

namespace GymGo.Application.Auth.DTOs;

/// <summary>
/// Respuesta exitosa del endpoint POST /auth/login.
/// Contiene el JWT y metadatos del usuario autenticado.
/// </summary>
public sealed record LoginResponseDto(
    string Token,
    DateTime ExpiresAtUtc,
    Guid UserId,
    string FullName,
    string Email,
    UserRole Role,
    Guid TenantId
);
