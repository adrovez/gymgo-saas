using GymGo.Application.ClassReservations.DTOs;
using MediatR;

namespace GymGo.Application.ClassReservations.Queries.GetReservationsBySession;

/// <summary>
/// Devuelve todas las reservas de una sesión concreta (ClassScheduleId + SessionDate).
/// </summary>
/// <param name="ClassScheduleId">Id del horario semanal.</param>
/// <param name="SessionDate">Fecha exacta de la sesión.</param>
public sealed record GetReservationsBySessionQuery(
    Guid ClassScheduleId,
    DateOnly SessionDate
) : IRequest<IReadOnlyList<ClassReservationDto>>;
