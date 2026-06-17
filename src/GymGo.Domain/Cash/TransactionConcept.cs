namespace GymGo.Domain.Cash;

public enum TransactionConcept
{
    // ── Ingresos (0–3) ────────────────────────────────────────────────────
    CuotaMembresia   = 0,
    Matricula        = 1,
    ProductoServicio = 2,
    OtroIngreso      = 3,

    // ── Egresos (10–13) ───────────────────────────────────────────────────
    Servicios        = 10,   // Luz, agua, gas, internet
    Mantencion       = 11,   // Técnicos, reparaciones
    Insumos          = 12,   // Materiales, limpieza
    OtroEgreso       = 13
}
