using GymGo.Application.Common.Interfaces;
using GymGo.Domain.ClassReservations;
using GymGo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.ClassReservations.Commands.CancelReservation;

public sealed class CancelReservationCommandHandler : IRequestHandler<CancelReservationCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTimeProvider _dateTime;

    public CancelReservationCommandHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser,
        IDateTimeProvider dateTime)
    {
        _context     = context;
        _currentUser = currentUser;
        _dateTime    = dateTime;
    }

    public async Task Handle(CancelReservationCommand request, CancellationToken cancellationToken)
    {
        // ── 1. Localizar la reserva (el query filter ya aplica tenant) ────────
        var reservation = await _context.ClassReservations
            .FirstOrDefaultAsync(r => r.Id == request.ReservationId, cancellationToken)
            ?? throw new NotFoundException("ClassReservation", request.ReservationId);

        // ── 2. Validar que el status de cancelación sea válido ────────────────
        if (request.CancelStatus != ReservationStatus.CancelledByMember
         && request.CancelStatus != ReservationStatus.CancelledByStaff)
            throw new BusinessRuleViolationException(
                "RESERVATION_CANCEL_STATUS_INVALID",
                "El estado de cancelación debe ser CancelledByMember o CancelledByStaff.");

        // ── 3. Cancelar (el dominio valida que esté Active) ───────────────────
        var cancelledBy = _currentUser.Email ?? "system";

        reservation.Cancel(
            cancelStatus:    request.CancelStatus,
            cancelledAtUtc:  _dateTime.UtcNow,
            cancelledBy:     cancelledBy,
            reason:          request.Reason);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
