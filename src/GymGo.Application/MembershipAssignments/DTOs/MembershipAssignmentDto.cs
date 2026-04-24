using GymGo.Domain.MembershipAssignments;

namespace GymGo.Application.MembershipAssignments.DTOs;

/// <summary>Detalle completo de una asignación de membresía.</summary>
public sealed record MembershipAssignmentDto(
    Guid Id,
    Guid TenantId,
    Guid MemberId,
    Guid MembershipPlanId,
    DateOnly StartDate,
    DateOnly EndDate,
    int DaysRemaining,
    decimal AmountSnapshot,
    AssignmentStatus Status,
    string StatusLabel,
    PaymentStatus PaymentStatus,
    string PaymentStatusLabel,
    DateTime? PaidAtUtc,
    DateOnly? FrozenSince,
    int FrozenDaysAccumulated,
    string? Notes,
    DateTime CreatedAtUtc,
    string? CreatedBy,
    DateTime? ModifiedAtUtc,
    string? ModifiedBy
);
