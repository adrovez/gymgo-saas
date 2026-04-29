using GymGo.Domain.ClassReservations;
using MediatR;

namespace GymGo.Application.ClassReservations.Commands.CancelReservation;

/// <summary>
/// Cancela una reserva existente.
/// </summary>
/// <param name="ReservationId">Id de la reserva a cancelar.</param>
/// <param name="CancelStatus">
///   CancelledByMember — el propio socio cancela.<br/>
///   CancelledByStaff  — el staff cancela en nombre del socio.
/// </param>
/// <param name="Reason">Motivo de cancelación (opcional).</param>
public sealed record CancelReservationCommand(
    Guid ReservationId,
    ReservationStatus CancelStatus = ReservationStatus.CancelledByMember,
    string? Reason = null
) : IRequest;
