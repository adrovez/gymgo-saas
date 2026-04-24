using GymGo.Domain.MembershipPlans;
using MediatR;

namespace GymGo.Application.MembershipPlans.Commands.UpdateMembershipPlan;

/// <summary>Comando para actualizar un plan existente.</summary>
public sealed record UpdateMembershipPlanCommand(
    Guid PlanId,
    string Name,
    string? Description,
    Periodicity Periodicity,
    int DaysPerWeek,
    bool FixedDays,
    bool Monday,
    bool Tuesday,
    bool Wednesday,
    bool Thursday,
    bool Friday,
    bool Saturday,
    bool Sunday,
    bool FreeSchedule,
    TimeOnly? TimeFrom,
    TimeOnly? TimeTo,
    decimal Amount,
    bool AllowsFreezing
) : IRequest;
