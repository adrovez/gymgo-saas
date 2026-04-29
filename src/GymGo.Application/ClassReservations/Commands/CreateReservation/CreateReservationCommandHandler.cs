using GymGo.Application.Common.Interfaces;
using GymGo.Domain.ClassReservations;
using GymGo.Domain.Exceptions;
using GymGo.Domain.Members;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.ClassReservations.Commands.CreateReservation;

public sealed class CreateReservationCommandHandler : IRequestHandler<CreateReservationCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenant _currentTenant;
    private readonly IDateTimeProvider _dateTime;
    public CreateReservationCommandHandler(
        IApplicationDbContext context,
        ICurrentTenant currentTenant,
        IDateTimeProvider dateTime)
    {
        _context       = context;
        _currentTenant = currentTenant;
        _dateTime      = dateTime;
    }

    public async Task<Guid> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _currentTenant.TenantId
            ?? throw new UnauthorizedAccessException("No se pudo determinar el tenant actual.");

        var nowUtc = _dateTime.UtcNow;
        var today  = DateOnly.FromDateTime(nowUtc);

        // ── 1. La fecha de la sesión no puede ser anterior a hoy ─────────────
        if (request.SessionDate < today)
            throw new BusinessRuleViolationException(
                "RESERVATION_SESSION_DATE_PAST",
                "No se puede reservar para una sesión que ya pasó.");

        // ── 2. Verificar que el socio existe y está activo ────────────────────
        var member = await _context.Members
            .FirstOrDefaultAsync(m => m.Id == request.MemberId, cancellationToken)
            ?? throw new NotFoundException("Member", request.MemberId);

        if (member.Status != MemberStatus.Active)
            throw new BusinessRuleViolationException(
                "RESERVATION_MEMBER_NOT_ACTIVE",
                "El socio no está activo y no puede hacer reservas.");

        // ── 3. Verificar que el horario existe, está activo y su clase activa ─
        var schedule = await _context.ClassSchedules
            .Include(s => s.GymClass)
            .FirstOrDefaultAsync(s => s.Id == request.ClassScheduleId, cancellationToken)
            ?? throw new NotFoundException("ClassSchedule", request.ClassScheduleId);

        if (!schedule.IsActive)
            throw new BusinessRuleViolationException(
                "RESERVATION_SCHEDULE_INACTIVE",
                "El horario de clase no está activo.");

        if (!schedule.GymClass.IsActive)
            throw new BusinessRuleViolationException(
                "RESERVATION_CLASS_INACTIVE",
                "La clase asociada al horario no está activa.");

        // ── 4. La fecha debe coincidir con el día del horario ─────────────────
        if (request.SessionDate.DayOfWeek != schedule.DayOfWeek)
            throw new BusinessRuleViolationException(
                "RESERVATION_DATE_DAY_MISMATCH",
                $"La fecha indicada ({request.SessionDate:dd/MM/yyyy}) " +
                $"no corresponde al día '{schedule.DayOfWeek.ToSpanish()}' del horario.");

        // ── 5. No puede haber reserva activa duplicada ────────────────────────
        var duplicate = await _context.ClassReservations
            .AnyAsync(r =>
                r.MemberId        == request.MemberId       &&
                r.ClassScheduleId == request.ClassScheduleId &&
                r.SessionDate     == request.SessionDate    &&
                r.Status          == ReservationStatus.Active,
                cancellationToken);

        if (duplicate)
            throw new BusinessRuleViolationException(
                "RESERVATION_DUPLICATE",
                "El socio ya tiene una reserva activa para esta sesión.");

        // ── 6. Verificar capacidad ────────────────────────────────────────────
        var effectiveCapacity = schedule.MaxCapacity ?? schedule.GymClass.MaxCapacity;

        var activeCount = await _context.ClassReservations
            .CountAsync(r =>
                r.ClassScheduleId == request.ClassScheduleId &&
                r.SessionDate     == request.SessionDate     &&
                r.Status          == ReservationStatus.Active,
                cancellationToken);

        if (activeCount >= effectiveCapacity)
            throw new BusinessRuleViolationException(
                "RESERVATION_SESSION_FULL",
                $"La sesión ya no tiene cupos disponibles " +
                $"({activeCount}/{effectiveCapacity}).");

        // ── 7. Crear la reserva ───────────────────────────────────────────────
        var reservation = ClassReservation.Create(
            tenantId:        tenantId,
            memberId:        member.Id,
            classScheduleId: schedule.Id,
            sessionDate:     request.SessionDate,
            reservedAtUtc:   nowUtc,
            memberFullName:  $"{member.FirstName} {member.LastName}",
            notes:           request.Notes);

        _context.ClassReservations.Add(reservation);
        await _context.SaveChangesAsync(cancellationToken);

        return reservation.Id;
    }
}

/// <summary>Extensiones de traducción para DayOfWeek.</summary>
internal static class DayOfWeekSpanishExtensions
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
