using GymGo.Domain.MembershipPlans;

namespace GymGo.Application.MembershipPlans.DTOs;

public static class MembershipPlanMappings
{
    public static MembershipPlanDto ToDto(this MembershipPlan p) => new(
        Id:               p.Id,
        TenantId:         p.TenantId,
        Name:             p.Name,
        Description:      p.Description,
        Periodicity:      p.Periodicity,
        PeriodicityLabel: p.Periodicity.ToLabel(),
        DurationDays:     p.DurationDays,
        DaysPerWeek:      p.DaysPerWeek,
        FixedDays:        p.FixedDays,
        Monday:           p.Monday,
        Tuesday:          p.Tuesday,
        Wednesday:        p.Wednesday,
        Thursday:         p.Thursday,
        Friday:           p.Friday,
        Saturday:         p.Saturday,
        Sunday:           p.Sunday,
        DaysLabel:        BuildDaysLabel(p),
        FreeSchedule:     p.FreeSchedule,
        TimeFrom:         p.TimeFrom,
        TimeTo:           p.TimeTo,
        ScheduleLabel:    BuildScheduleLabel(p),
        Amount:           p.Amount,
        AllowsFreezing:   p.AllowsFreezing,
        IsActive:         p.IsActive,
        DeactivatedAtUtc: p.DeactivatedAtUtc,
        CreatedAtUtc:     p.CreatedAtUtc,
        CreatedBy:        p.CreatedBy,
        ModifiedAtUtc:    p.ModifiedAtUtc,
        ModifiedBy:       p.ModifiedBy
    );

    public static MembershipPlanSummaryDto ToSummaryDto(this MembershipPlan p) => new(
        Id:               p.Id,
        Name:             p.Name,
        Periodicity:      p.Periodicity,
        PeriodicityLabel: p.Periodicity.ToLabel(),
        DurationDays:     p.DurationDays,
        DaysPerWeek:      p.DaysPerWeek,
        DaysLabel:        BuildDaysLabel(p),
        ScheduleLabel:    BuildScheduleLabel(p),
        Amount:           p.Amount,
        AllowsFreezing:   p.AllowsFreezing,
        IsActive:         p.IsActive
    );

    private static string ToLabel(this Periodicity p) => p switch
    {
        Periodicity.Monthly   => "Mensual",
        Periodicity.Quarterly => "Trimestral",
        Periodicity.Biannual  => "Semestral",
        Periodicity.Annual    => "Anual",
        _                     => p.ToString()
    };

    private static string BuildDaysLabel(MembershipPlan p)
    {
        if (p.DaysPerWeek == 7 && !p.FixedDays)
            return "Todos los días";

        if (!p.FixedDays)
            return $"{p.DaysPerWeek} veces por semana (días a elección)";

        var days = new List<string>();
        if (p.Monday)    days.Add("Lun");
        if (p.Tuesday)   days.Add("Mar");
        if (p.Wednesday) days.Add("Mié");
        if (p.Thursday)  days.Add("Jue");
        if (p.Friday)    days.Add("Vie");
        if (p.Saturday)  days.Add("Sáb");
        if (p.Sunday)    days.Add("Dom");

        return string.Join(", ", days);
    }

    private static string BuildScheduleLabel(MembershipPlan p)
    {
        if (p.FreeSchedule) return "Horario libre";

        var from = p.TimeFrom!.Value.ToString("HH:mm");
        var to   = p.TimeTo!.Value.ToString("HH:mm");
        return $"{from} – {to}";
    }
}
