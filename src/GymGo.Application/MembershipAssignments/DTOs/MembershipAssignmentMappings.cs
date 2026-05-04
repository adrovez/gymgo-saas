using GymGo.Domain.MembershipAssignments;

namespace GymGo.Application.MembershipAssignments.DTOs;

public static class MembershipAssignmentMappings
{
    public static MembershipAssignmentDto ToDto(this MembershipAssignment a) => new(
        Id:                     a.Id,
        TenantId:               a.TenantId,
        MemberId:               a.MemberId,
        MembershipPlanId:       a.MembershipPlanId,
        StartDate:              a.StartDate,
        EndDate:                a.EndDate,
        DaysRemaining:          CalculateDaysRemaining(a),
        AmountSnapshot:         a.AmountSnapshot,
        Status:                 a.Status,
        StatusLabel:            a.Status.ToLabel(),
        PaymentStatus:          a.PaymentStatus,
        PaymentStatusLabel:     a.PaymentStatus.ToLabel(),
        PaidAtUtc:              a.PaidAtUtc,
        FrozenSince:            a.FrozenSince,
        FrozenDaysAccumulated:  a.FrozenDaysAccumulated,
        Notes:                  a.Notes,
        CreatedAtUtc:           a.CreatedAtUtc,
        CreatedBy:              a.CreatedBy,
        ModifiedAtUtc:          a.ModifiedAtUtc,
        ModifiedBy:             a.ModifiedBy
    );

    /// <summary>
    /// Proyección básica sin datos de socio ni plan (para contextos donde ya son conocidos).
    /// Usa las sobrecargas con parámetros adicionales cuando se requieren nombre/RUT/plan.
    /// </summary>
    public static MembershipAssignmentSummaryDto ToSummaryDto(this MembershipAssignment a) =>
        a.ToSummaryDto(string.Empty, string.Empty, string.Empty);

    /// <summary>Proyección enriquecida con nombre del socio, RUT y nombre del plan.</summary>
    public static MembershipAssignmentSummaryDto ToSummaryDto(
        this MembershipAssignment a,
        string memberFullName,
        string memberRut,
        string planName) => new(
        Id:                 a.Id,
        MemberId:           a.MemberId,
        MemberFullName:     memberFullName,
        MemberRut:          memberRut,
        MembershipPlanId:   a.MembershipPlanId,
        PlanName:           planName,
        StartDate:          a.StartDate,
        EndDate:            a.EndDate,
        DaysRemaining:      CalculateDaysRemaining(a),
        AmountSnapshot:     a.AmountSnapshot,
        Status:             a.Status,
        StatusLabel:        a.Status.ToLabel(),
        PaymentStatus:      a.PaymentStatus,
        PaymentStatusLabel: a.PaymentStatus.ToLabel()
    );

    private static int CalculateDaysRemaining(MembershipAssignment a)
    {
        if (a.Status != AssignmentStatus.Active) return 0;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var diff  = a.EndDate.DayNumber - today.DayNumber;
        return Math.Max(diff, 0);
    }

    private static string ToLabel(this AssignmentStatus s) => s switch
    {
        AssignmentStatus.Active    => "Activa",
        AssignmentStatus.Expired   => "Vencida",
        AssignmentStatus.Cancelled => "Cancelada",
        AssignmentStatus.Frozen    => "Congelada",
        _                          => s.ToString()
    };

    private static string ToLabel(this PaymentStatus s) => s switch
    {
        PaymentStatus.Pending  => "Pendiente",
        PaymentStatus.Paid     => "Pagada",
        PaymentStatus.Overdue  => "Morosa",
        _                      => s.ToString()
    };
}
