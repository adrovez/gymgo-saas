namespace GymGo.Domain.Sales;

/// <summary>
/// Tipo de ítem dentro de una venta.
/// Persistido como INT en la base de datos.
/// </summary>
public enum SaleItemType
{
    /// <summary>Plan de membresía. Solo puede haber uno por venta.</summary>
    Plan = 0,

    /// <summary>Matrícula / inscripción al gimnasio.</summary>
    Matricula = 1,

    /// <summary>Servicio extra (ej: evaluación física, clase especial).</summary>
    ServicioExtra = 2,

    /// <summary>Otro concepto cobrable de descripción libre.</summary>
    Otro = 3
}
