namespace GymGo.Domain.Sales;

/// <summary>
/// Estado de ciclo de vida de una venta.
/// Persistido como INT en la base de datos.
/// </summary>
public enum SaleStatus
{
    /// <summary>En construcción. Aún no se ha cobrado.</summary>
    Draft = 0,

    /// <summary>Cobrada. Generó MembershipAssignment y CashTransaction.</summary>
    Confirmed = 1,

    /// <summary>Anulada. La CashTransaction asociada también fue anulada.</summary>
    Voided = 2
}
