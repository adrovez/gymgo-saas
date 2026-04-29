using GymGo.Application.Common.Interfaces;
using GymGo.Domain.ClassAttendances;
using GymGo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.ClassAttendances.Commands.CheckInMember;

public sealed class CheckInMemberCommandHandler : IRequestHandler<CheckInMemberCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenant _currentTenant;
    private readonly IDateTimeProvider _dateTime;

    public CheckInMemberCommandHandler(
        IApplicationDbContext context,
        ICurrentTenant currentTenant,
        IDateTimeProvider dateTime)
    {
        _context       = context;
        _currentTenant = currentTenant;
        _dateTime      = dateTime;
    }

    public async Task<Guid> Handle(CheckInMemberCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _currentTenant.TenantId
            ?? throw new UnauthorizedAccessException("No se pudo determinar el tenant actual.");

        // ── 1. Verificar que el socio existe y pertenece al tenant ────────────
        var member = await _context.Members
            .FirstOrDefaultAsync(m => m.Id == request.MemberId, cancellationToken)
            ?? throw new NotFoundException("Member", request.MemberId);

        // ── 2. Verificar que el horario existe y pertenece al tenant ──────────
        var schedule = await _context.ClassSchedules
            .FirstOrDefaultAsync(s => s.Id == request.ClassScheduleId, cancellationToken)
            ?? throw new NotFoundException("ClassSchedule", request.ClassScheduleId);

        // ── 3. Determinar la fecha de la sesión ───────────────────────────────
        var nowUtc       = _dateTime.UtcNow;
        var sessionDate  = request.SessionDate ?? DateOnly.FromDateTime(nowUtc);

        // ── 4. Validar que no exista ya un check-in para esta sesión ──────────
        var duplicate = await _context.ClassAttendances
            .AnyAsync(
                a => a.MemberId         == request.MemberId
                  && a.ClassScheduleId  == request.ClassScheduleId
                  && a.SessionDate      == sessionDate,
                cancellationToken);

        if (duplicate)
            throw new BusinessRuleViolationException(
                "ATTENDANCE_DUPLICATE",
                $"El socio '{member.FirstName} {member.LastName}' ya tiene check-in registrado " +
                $"para esta sesión el {sessionDate:dd/MM/yyyy}.");

        // ── 5. Registrar el check-in ──────────────────────────────────────────
        var attendance = ClassAttendance.Create(
            tenantId:        tenantId,
            memberId:        member.Id,
            classScheduleId: schedule.Id,
            sessionDate:     sessionDate,
            checkedInAtUtc:  nowUtc,
            checkInMethod:   request.CheckInMethod,
            memberFullName:  $"{member.FirstName} {member.LastName}",
            notes:           request.Notes);

        _context.ClassAttendances.Add(attendance);
        await _context.SaveChangesAsync(cancellationToken);

        return attendance.Id;
    }
}
