namespace GymGo.Domain.Maintenance;

/// <summary>
/// Tipo de mantención de maquinaria.
/// Persistido como int en la base de datos.
/// </summary>
public enum MaintenanceType
{
    /// <summary>Mantención programada / periódica para prevenir fallas.</summary>
    Preventive = 0,

    /// <summary>Reparación ante falla o daño reportado.</summary>
    Corrective = 1,
}
