namespace GymGo.Domain.Maintenance;

/// <summary>
/// Estado de ciclo de vida de un registro de mantención.
/// Persistido como int en la base de datos.
///
/// Flujo válido:
///   Pending → InProgress → Completed
///                        ↘ Cancelled
///   Pending → Cancelled
/// </summary>
public enum MaintenanceStatus
{
    /// <summary>Registrada, aún no iniciada.</summary>
    Pending    = 0,

    /// <summary>En ejecución.</summary>
    InProgress = 1,

    /// <summary>Finalizada con éxito.</summary>
    Completed  = 2,

    /// <summary>Cancelada antes de completarse.</summary>
    Cancelled  = 3,
}
