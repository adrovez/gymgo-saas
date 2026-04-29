namespace GymGo.Domain.Maintenance;

/// <summary>
/// Indica si la mantención es ejecutada por staff interno o por proveedor externo.
/// Persistido como int en la base de datos.
/// </summary>
public enum ResponsibleType
{
    /// <summary>Staff interno del gimnasio registrado en el sistema.</summary>
    Internal = 0,

    /// <summary>Proveedor o técnico externo (empresa de servicio, etc.).</summary>
    External = 1,
}
