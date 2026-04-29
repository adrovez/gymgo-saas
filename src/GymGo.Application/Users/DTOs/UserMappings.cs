using GymGo.Domain.Users;

namespace GymGo.Application.Users.DTOs;

/// <summary>
/// Métodos de mapeo estáticos de la entidad <see cref="User"/> a los DTOs de lectura.
/// </summary>
public static class UserMappings
{
    public static UserDto ToDto(this User u) => new(
        Id:           u.Id,
        TenantId:     u.TenantId,
        Email:        u.Email,
        FullName:     u.FullName,
        Role:         u.Role,
        RoleLabel:    u.Role.ToLabel(),
        IsActive:     u.IsActive,
        LastLoginUtc: u.LastLoginUtc,
        CreatedAtUtc: u.CreatedAtUtc,
        CreatedBy:    u.CreatedBy,
        ModifiedAtUtc: u.ModifiedAtUtc,
        ModifiedBy:   u.ModifiedBy
    );

    public static UserSummaryDto ToSummaryDto(this User u) => new(
        Id:           u.Id,
        Email:        u.Email,
        FullName:     u.FullName,
        Role:         u.Role,
        RoleLabel:    u.Role.ToLabel(),
        IsActive:     u.IsActive,
        LastLoginUtc: u.LastLoginUtc,
        CreatedAtUtc: u.CreatedAtUtc
    );

    public static string ToLabel(this UserRole role) => role switch
    {
        UserRole.PlatformAdmin => "Platform Admin",
        UserRole.GymOwner      => "Dueño",
        UserRole.GymStaff      => "Staff",
        UserRole.Instructor    => "Instructor",
        UserRole.Member        => "Socio",
        _                      => role.ToString()
    };
}
