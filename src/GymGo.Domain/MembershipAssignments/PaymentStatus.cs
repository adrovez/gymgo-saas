namespace GymGo.Domain.MembershipAssignments;

/// <summary>
/// Estado de pago de una asignación de membresía.
/// Persistido como INT en la base de datos.
/// </summary>
public enum PaymentStatus
{
    /// <summary>Asignada, pago aún no registrado.</summary>
    Pending = 0,

    /// <summary>Pago confirmado y registrado.</summary>
    Paid = 1,

    /// <summary>Plazo vencido sin pago. Gatilla Delinquent en el socio.</summary>
    Overdue = 2
}
