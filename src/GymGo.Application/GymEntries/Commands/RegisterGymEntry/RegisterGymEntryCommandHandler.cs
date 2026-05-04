using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Exceptions;
using GymGo.Domain.GymEntries;
using GymGo.Domain.Members;
using GymGo.Domain.MembershipAssignments;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.GymEntries.Commands.RegisterGymEntry;

public sealed class RegisterGymEntryCommandHandler : IRequestHandler<RegisterGymEntryCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenant _currentTenant;
    private readonly IDateTimeProvider _dateTime;

    public RegisterGymEntryCommandHandler(
        IApplicationDbContext context,
        ICurrentTenant currentTenant,
        IDateTimeProvider dateTime)
    {
        _context       = context;
        _currentTenant = currentTenant;
        _dateTime      = dateTime;
    }

    public async Task<Guid> Handle(RegisterGymEntryCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _currentTenant.TenantId
            ?? throw new UnauthorizedAccessException("No se pudo determinar el tenant actual.");

        var nowUtc    = _dateTime.UtcNow;
        var today     = DateOnly.FromDateTime(nowUtc);
        var timeNow   = TimeOnly.FromDateTime(nowUtc);

        // ── 1. Verificar que el socio existe y pertenece al tenant ────────────
        var member = await _context.Members
            .FirstOrDefaultAsync(m => m.Id == request.MemberId, cancellationToken)
            ?? throw new NotFoundException("Member", request.MemberId);

        // ── 2. Verificar que el socio está activo ─────────────────────────────
        if (member.Status != MemberStatus.Active)
        {
            var reason = member.Status == MemberStatus.Suspended
                ? "El socio se encuentra suspendido y no puede ingresar al gimnasio."
                : "El socio se encuentra en estado moroso y no puede ingresar al gimnasio.";

            throw new BusinessRuleViolationException("ENTRY_MEMBER_NOT_ACTIVE", reason);
        }

        // ── 3. Buscar membresía activa y vigente ──────────────────────────────
        var assignment = await _context.MembershipAssignments
            .Where(a =>
                a.MemberId == request.MemberId
                && a.Status == AssignmentStatus.Active
                && a.EndDate >= today)
            .OrderByDescending(a => a.StartDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (assignment is null)
            throw new BusinessRuleViolationException(
                "ENTRY_NO_ACTIVE_MEMBERSHIP",
                "El socio no tiene una membresía activa y vigente.");

        // ── 4. Cargar el plan de membresía ────────────────────────────────────
        var plan = await _context.MembershipPlans
            .FirstOrDefaultAsync(p => p.Id == assignment.MembershipPlanId, cancellationToken)
            ?? throw new NotFoundException("MembershipPlan", assignment.MembershipPlanId);

        // ── 5. Validar restricción de días ────────────────────────────────────
        if (plan.FixedDays)
        {
            var allowedToday = today.DayOfWeek switch
            {
                DayOfWeek.Monday    => plan.Monday,
                DayOfWeek.Tuesday   => plan.Tuesday,
                DayOfWeek.Wednesday => plan.Wednesday,
                DayOfWeek.Thursday  => plan.Thursday,
                DayOfWeek.Friday    => plan.Friday,
                DayOfWeek.Saturday  => plan.Saturday,
                DayOfWeek.Sunday    => plan.Sunday,
                _                   => false
            };

            if (!allowedToday)
            {
                var allowedDayNames = BuildAllowedDaysMessage(plan);
                throw new BusinessRuleViolationException(
                    "ENTRY_DAY_NOT_ALLOWED",
                    $"El plan '{plan.Name}' no permite acceso los {today.DayOfWeek.ToSpanish()}. " +
                    $"Días habilitados: {allowedDayNames}.");
            }
        }

        // ── 6. Validar restricción de horario ─────────────────────────────────
        if (!plan.FreeSchedule && plan.TimeFrom.HasValue && plan.TimeTo.HasValue)
        {
            if (timeNow < plan.TimeFrom.Value || timeNow > plan.TimeTo.Value)
                throw new BusinessRuleViolationException(
                    "ENTRY_TIME_NOT_ALLOWED",
                    $"El plan '{plan.Name}' solo permite acceso entre " +
                    $"{plan.TimeFrom.Value:HH:mm} y {plan.TimeTo.Value:HH:mm}. " +
                    $"Hora actual: {timeNow:HH:mm}.");
        }

        // ── 7. Verificar que el socio no haya ingresado ya hoy ───────────────
        var alreadyEnteredToday = await _context.GymEntries
            .AnyAsync(e => e.MemberId == request.MemberId && e.EntryDate == today, cancellationToken);

        if (alreadyEnteredToday)
            throw new BusinessRuleViolationException(
                "ENTRY_ALREADY_REGISTERED_TODAY",
                $"El socio '{member.FirstName} {member.LastName}' ya registró su ingreso hoy. " +
                "No se puede registrar un segundo ingreso para el mismo día.");

        // ── 8. Registrar el ingreso ───────────────────────────────────────────
        var entry = GymEntry.Create(
            tenantId:               tenantId,
            memberId:               member.Id,
            membershipAssignmentId: assignment.Id,
            entryDate:              today,
            enteredAtUtc:           nowUtc,
            method:                 request.Method,
            memberFullName:         $"{member.FirstName} {member.LastName}",
            notes:                  request.Notes);

        _context.GymEntries.Add(entry);
        await _context.SaveChangesAsync(cancellationToken);

        return entry.Id;
    }

    // ── Helpers privados ──────────────────────────────────────────────────

    private static string BuildAllowedDaysMessage(Domain.MembershipPlans.MembershipPlan plan)
    {
        var days = new List<string>();
        if (plan.Monday)    days.Add("Lunes");
        if (plan.Tuesday)   days.Add("Martes");
        if (plan.Wednesday) days.Add("Miércoles");
        if (plan.Thursday)  days.Add("Jueves");
        if (plan.Friday)    days.Add("Viernes");
        if (plan.Saturday)  days.Add("Sábado");
        if (plan.Sunday)    days.Add("Domingo");
        return string.Join(", ", days);
    }
}

/// <summary>Extension para traducir DayOfWeek a español.</summary>
internal static class DayOfWeekExtensions
{
    internal static string ToSpanish(this DayOfWeek day) => day switch
    {
        DayOfWeek.Monday    => "Lunes",
        DayOfWeek.Tuesday   => "Martes",
        DayOfWeek.Wednesday => "Miércoles",
        DayOfWeek.Thursday  => "Jueves",
        DayOfWeek.Friday    => "Viernes",
        DayOfWeek.Saturday  => "Sábado",
        DayOfWeek.Sunday    => "Domingo",
        _                   => day.ToString()
    };
}
