using GymGo.Application.ClassAttendances.DTOs;
using MediatR;

namespace GymGo.Application.ClassAttendances.Queries.GetAttendancesBySession;

/// <summary>
/// Devuelve todos los check-ins de un horario para una fecha concreta.
/// </summary>
/// <param name="ClassScheduleId">Id del horario semanal.</param>
/// <param name="SessionDate">Fecha de la sesión. Si es null, se usa la fecha UTC actual.</param>
public sealed record GetAttendancesBySessionQuery(
    Guid ClassScheduleId,
    DateOnly? SessionDate
) : IRequest<IReadOnlyList<ClassAttendanceDto>>;
