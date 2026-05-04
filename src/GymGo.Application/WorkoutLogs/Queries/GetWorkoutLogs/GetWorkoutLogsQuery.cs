using GymGo.Application.WorkoutLogs.DTOs;
using MediatR;

namespace GymGo.Application.WorkoutLogs.Queries.GetWorkoutLogs;

/// <summary>
/// Retorna el historial de sesiones de entrenamiento de un socio,
/// filtrado opcionalmente por rango de fechas. Ordenado por fecha descendente.
/// </summary>
/// <param name="MemberId">Socio del que se quiere el historial.</param>
/// <param name="From">Fecha inicial del rango (inclusive). Null = sin límite inferior.</param>
/// <param name="To">Fecha final del rango (inclusive). Null = sin límite superior.</param>
public sealed record GetWorkoutLogsQuery(
    Guid MemberId,
    DateOnly? From,
    DateOnly? To
) : IRequest<IReadOnlyList<WorkoutLogSummaryDto>>;
