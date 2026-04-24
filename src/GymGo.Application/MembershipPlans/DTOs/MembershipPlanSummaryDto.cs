using GymGo.Domain.MembershipPlans;

namespace GymGo.Application.MembershipPlans.DTOs;

/// <summary>Vista resumida de un plan para listados.</summary>
public sealed record MembershipPlanSummaryDto(
    Guid Id,
    string Name,
    Periodicity Periodicity,
    string PeriodicityLabel,
    int DurationDays,
    int DaysPerWeek,
    string DaysLabel,
    string ScheduleLabel,
    decimal Amount,
    bool AllowsFreezing,
    bool IsActive
);
