using GymGo.Domain.MembershipPlans;

namespace GymGo.Application.MembershipPlans.DTOs;

/// <summary>Detalle completo de un plan de membresía.</summary>
public sealed record MembershipPlanDto(
    Guid Id,
    Guid TenantId,
    string Name,
    string? Description,
    Periodicity Periodicity,
    string PeriodicityLabel,
    int DurationDays,
    int DaysPerWeek,
    bool FixedDays,
    bool Monday,
    bool Tuesday,
    bool Wednesday,
    bool Thursday,
    bool Friday,
    bool Saturday,
    bool Sunday,
    string DaysLabel,
    bool FreeSchedule,
    TimeOnly? TimeFrom,
    TimeOnly? TimeTo,
    string ScheduleLabel,
    decimal Amount,
    bool AllowsFreezing,
    bool IsActive,
    DateTime? DeactivatedAtUtc,
    DateTime CreatedAtUtc,
    string? CreatedBy,
    DateTime? ModifiedAtUtc,
    string? ModifiedBy
);
