using GymGo.Domain.MembershipPlans;
using MediatR;

namespace GymGo.Application.MembershipPlans.Commands.CreateMembershipPlan;

/// <summary>
/// Comando para crear un nuevo plan de membresía.
/// El TenantId lo inyecta el handler desde ICurrentTenant.
/// </summary>
public sealed record CreateMembershipPlanCommand(
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
) : IRequest<Guid>;
