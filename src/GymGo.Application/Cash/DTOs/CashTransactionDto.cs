namespace GymGo.Application.Cash.DTOs;

public sealed record CashTransactionDto(
    Guid     Id,
    DateOnly Date,
    string   Type,            // "Ingreso" | "Egreso"
    decimal  Amount,
    string   PaymentMethod,   // "Efectivo" | "Tarjeta" | "Transferencia"
    string   Concept,         // "CuotaMembresia" | "Servicios" | …
    string?  Description,
    Guid?    MemberId,
    string?  MemberFullName,
    Guid?    MembershipAssignmentId,
    bool     IsVoided,
    DateTime? VoidedAtUtc,
    string?  VoidReason,
    DateTime CreatedAtUtc
);
