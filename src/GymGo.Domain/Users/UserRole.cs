namespace GymGo.Domain.Users;

/// <summary>
/// Roles base del sistema. La autorización fina (permisos) puede
/// agregarse más adelante; por ahora roles atómicos.
/// </summary>
public enum UserRole
{
    /// <summary>Soporte interno de GymGo. No pertenece a un tenant.</summary>
    PlatformAdmin = 0,

    /// <summary>Dueño del gimnasio.</summary>
    GymOwner = 1,

    /// <summary>Personal administrativo del gimnasio.</summary>
    GymStaff = 2,

    /// <summary>Instructor / entrenador.</summary>
    Instructor = 3,

    /// <summary>Socio del gimnasio (acceso a la app móvil).</summary>
    Member = 4
}
