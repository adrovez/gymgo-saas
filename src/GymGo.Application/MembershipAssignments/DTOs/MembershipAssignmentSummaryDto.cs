using GymGo.Domain.MembershipAssignments;

namespace GymGo.Application.MembershipAssignments.DTOs;

/// <summary>Vista resumida para listados de asignaciones.</summary>
public sealed record MembershipAssignmentSummaryDto(
    Guid Id,
    Guid MemberId,
    /// <summary>Nombre completo del socio. Vacío cuando el socio ya es conocido por contexto.</summary>
    string MemberFullName,
    /// <summary>RUT del socio. Vacío cuando el socio ya es conocido por contexto.</summary>
    string MemberRut,
    Guid MembershipPlanId,
    /// <summary>Nombre del plan de membresía.</summary>
    string PlanName,
    DateOnly StartDate,
    DateOnly EndDate,
    int DaysRemaining,
    decimal AmountSnapshot,
    AssignmentStatus Status,
    string StatusLabel,
    PaymentStatus PaymentStatus,
    string PaymentStatusLabel
);
