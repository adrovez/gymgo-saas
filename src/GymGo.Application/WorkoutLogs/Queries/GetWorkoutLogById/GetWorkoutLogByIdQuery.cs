using GymGo.Application.WorkoutLogs.DTOs;
using MediatR;

namespace GymGo.Application.WorkoutLogs.Queries.GetWorkoutLogById;

/// <summary>
/// Retorna el detalle completo de una sesión de entrenamiento con todos sus ejercicios.
/// </summary>
public sealed record GetWorkoutLogByIdQuery(Guid Id) : IRequest<WorkoutLogDto>;
