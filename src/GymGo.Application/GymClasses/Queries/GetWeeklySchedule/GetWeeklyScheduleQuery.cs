using GymGo.Application.GymClasses.DTOs;
using MediatR;

namespace GymGo.Application.GymClasses.Queries.GetWeeklySchedule;

/// <summary>
/// Devuelve todos los horarios activos del tenant agrupados por día de la semana.
/// Útil para renderizar el calendario semanal.
/// </summary>
public sealed record GetWeeklyScheduleQuery() : IRequest<IReadOnlyList<ClassScheduleDto>>;
