using MediatR;

namespace GymGo.Application.ClassReservations.Commands.CreateReservation;

/// <summary>
/// Crea una reserva para un socio en una sesión concreta de un horario de clase.
/// </summary>
/// <param name="MemberId">Id del socio que reserva.</param>
/// <param name="ClassScheduleId">Id del horario semanal (ClassSchedule).</param>
/// <param name="SessionDate">Fecha exacta de la sesión a reservar.</param>
/// <param name="Notes">Observaciones opcionales.</param>
public sealed record CreateReservationCommand(
    Guid MemberId,
    Guid ClassScheduleId,
    DateOnly SessionDate,
    string? Notes = null
) : IRequest<Guid>;
