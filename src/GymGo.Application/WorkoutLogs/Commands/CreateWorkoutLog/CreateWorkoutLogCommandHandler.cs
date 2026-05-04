using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Exceptions;
using GymGo.Domain.WorkoutLogs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.WorkoutLogs.Commands.CreateWorkoutLog;

public sealed class CreateWorkoutLogCommandHandler : IRequestHandler<CreateWorkoutLogCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenant _currentTenant;

    public CreateWorkoutLogCommandHandler(IApplicationDbContext context, ICurrentTenant currentTenant)
    {
        _context       = context;
        _currentTenant = currentTenant;
    }

    public async Task<Guid> Handle(CreateWorkoutLogCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _currentTenant.TenantId
            ?? throw new UnauthorizedAccessException("No se pudo determinar el tenant actual.");

        // ── 1. Verificar que el socio existe y pertenece al tenant ────────────
        var memberExists = await _context.Members
            .AnyAsync(m => m.Id == request.MemberId, cancellationToken);

        if (!memberExists)
            throw new NotFoundException("Member", request.MemberId);

        // ── 2. Verificar que no exista ya un log Draft para el mismo día ──────
        var sessionDate = request.Date ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var existingDraft = await _context.WorkoutLogs
            .AnyAsync(w =>
                w.MemberId == request.MemberId
                && w.Date   == sessionDate
                && w.Status == WorkoutLogStatus.Draft,
                cancellationToken);

        if (existingDraft)
            throw new BusinessRuleViolationException(
                "WORKOUTLOG_DRAFT_ALREADY_EXISTS",
                $"El socio ya tiene una sesión de entrenamiento en curso para el {sessionDate:dd/MM/yyyy}. " +
                "Completa o elimina la sesión existente antes de crear una nueva para ese día.");

        // ── 3. Crear el log ───────────────────────────────────────────────────
        var log = WorkoutLog.Create(
            tenantId: tenantId,
            memberId: request.MemberId,
            date:     request.Date,
            title:    request.Title,
            notes:    request.Notes);

        _context.WorkoutLogs.Add(log);
        await _context.SaveChangesAsync(cancellationToken);

        return log.Id;
    }
}
