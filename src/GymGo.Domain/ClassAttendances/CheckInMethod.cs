namespace GymGo.Domain.ClassAttendances;

/// <summary>
/// Método utilizado para registrar el check-in del socio.
/// </summary>
public enum CheckInMethod
{
    /// <summary>Registro manual por la recepcionista (búsqueda por nombre o RUT).</summary>
    Manual = 0,

    /// <summary>Escaneo del código QR del socio en recepción.</summary>
    QR = 1
}
