using GymGo.Application.ClassReservations.DTOs;
using GymGo.Domain.ClassReservations;
using MediatR;

namespace GymGo.Application.ClassReservations.Queries.GetReservationsByMember;

/// <summary>
/// Devuelve las reservas de un socio.
/// Opcionalmente filtrado por estado y/o rango de fechas.
/// </summary>
/// <param name="MemberId">Id del socio.</param>
/// <param name="Status">Filtrar por estado. Null = todos.</param>
/// <param name="From">Fecha de inicio (SessionDate). Null = sin límite inferior.</param>
/// <param name="To">Fecha de fin (SessionDate). Null = sin límite superior.</param>
public sealed record GetReservationsByMemberQuery(
    Guid MemberId,
    ReservationStatus? Status = null,
    DateOnly? From = null,
    DateOnly? To = null
) : IRequest<IReadOnlyList<ClassReservationDto>>;
