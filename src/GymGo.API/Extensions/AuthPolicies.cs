namespace GymGo.API.Extensions;

/// <summary>
/// Nombres de las políticas de autorización registradas en
/// <see cref="AuthenticationExtensions.AddJwtAuthentication"/>.
/// Usar estas constantes al llamar a .RequireAuthorization(policy).
/// </summary>
public static class AuthPolicies
{
    /// <summary>Solo PlatformAdmin y GymOwner.</summary>
    public const string AdminOrOwner = "AdminOrOwner";

    /// <summary>PlatformAdmin, GymOwner y GymStaff.</summary>
    public const string StaffOrAbove = "StaffOrAbove";

    /// <summary>PlatformAdmin, GymOwner, GymStaff e Instructor.</summary>
    public const string InstructorOrAbove = "InstructorOrAbove";
}
