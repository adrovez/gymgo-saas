namespace GymGo.Domain.MembershipAssignments;

/// <summary>
/// Estado operacional de una asignación de membresía.
/// Persistido como INT en la base de datos.
/// </summary>
public enum AssignmentStatus
{
    /// <summary>Membresía vigente dentro del período contratado.</summary>
    Active = 0,

    /// <summary>Período vencido sin renovación.</summary>
    Expired = 1,

    /// <summary>Cancelada manualmente antes del vencimiento.</summary>
    Cancelled = 2,

    /// <summary>Pausada temporalmente (solo si el plan permite congelamiento).</summary>
    Frozen = 3
}
