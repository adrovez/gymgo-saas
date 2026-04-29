namespace GymGo.Domain.ClassReservations;

/// <summary>
/// Estado de una reserva de clase.
/// </summary>
public enum ReservationStatus
{
    /// <summary>Reserva activa — el socio tiene un lugar confirmado.</summary>
    Active = 0,

    /// <summary>Cancelada por el propio socio.</summary>
    CancelledByMember = 1,

    /// <summary>Cancelada por el staff del gimnasio.</summary>
    CancelledByStaff = 2,

    /// <summary>El socio no se presentó a la clase.</summary>
    NoShow = 3,
}
