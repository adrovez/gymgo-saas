namespace GymGo.Domain.GymEntries;

/// <summary>
/// Método utilizado para registrar el ingreso al gimnasio.
/// </summary>
public enum GymEntryMethod
{
    /// <summary>Registro manual por recepcionista.</summary>
    Manual = 0,

    /// <summary>Escaneo de código QR por el socio.</summary>
    QR = 1,

    /// <summary>Tarjeta o llavero RFID/NFC.</summary>
    Badge = 2
}
