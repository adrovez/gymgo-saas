namespace GymGo.Domain.MembershipPlans;

/// <summary>
/// Periodicidad del plan de membresía. Determina la duración en días
/// y el ciclo de facturación.
/// Persistido como INT en la base de datos.
/// </summary>
public enum Periodicity
{
    /// <summary>Plan mensual — 30 días.</summary>
    Monthly = 1,

    /// <summary>Plan trimestral — 90 días.</summary>
    Quarterly = 2,

    /// <summary>Plan semestral — 180 días.</summary>
    Biannual = 3,

    /// <summary>Plan anual — 365 días.</summary>
    Annual = 4
}
