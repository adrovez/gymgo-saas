using GymGo.Domain.Users;

namespace GymGo.Application.Users.DTOs;

/// <summary>
/// Representación completa de un usuario para GET /users/{id}.
/// </summary>
public sealed record UserDto(
    Guid Id,
    Guid TenantId,
    string Email,
    string FullName,
    UserRole Role,
    string RoleLabel,
    bool IsActive,
    DateTime? LastLoginUtc,
    DateTime CreatedAtUtc,
    string? CreatedBy,
    DateTime? ModifiedAtUtc,
    string? ModifiedBy
);

/// <summary>
/// Representación resumida de un usuario para el listado.
/// </summary>
public sealed record UserSummaryDto(
    Guid Id,
    string Email,
    string FullName,
    UserRole Role,
    string RoleLabel,
    bool IsActive,
    DateTime? LastLoginUtc,
    DateTime CreatedAtUtc
);
