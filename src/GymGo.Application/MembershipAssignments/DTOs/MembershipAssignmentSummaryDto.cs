using GymGo.Domain.MembershipAssignments;

namespace GymGo.Application.MembershipAssignments.DTOs;

/// <summary>Vista resumida para listados de asignaciones.</summary>
public sealed record MembershipAssignmentSummaryDto(
    Guid Id,
    Guid MemberId,
    Guid MembershipPlanId,
    DateOnly StartDate,
    DateOnly EndDate,
    int DaysRemaining,
    decimal AmountSnapshot,
    AssignmentStatus Status,
    string StatusLabel,
    PaymentStatus PaymentStatus,
    string PaymentStatusLabel
);
