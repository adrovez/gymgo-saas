namespace GymGo.Domain.Members;

/// <summary>
/// Estado operacional del socio dentro del gimnasio.
/// Persistido como INT en la base de datos.
/// </summary>
public enum MemberStatus
{
    /// <summary>Socio activo con membresía vigente.</summary>
    Active = 0,

    /// <summary>Socio suspendido manualmente por el staff (ej. incumplimiento de reglamento).</summary>
    Suspended = 1,

    /// <summary>Socio con cuotas impagas. Generado automáticamente por el sistema de pagos.</summary>
    Delinquent = 2
}
