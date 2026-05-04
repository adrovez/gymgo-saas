using MediatR;

namespace GymGo.Application.WorkoutLogs.Commands.DeleteWorkoutLog;

/// <summary>
/// Elimina (soft delete) una sesión de entrenamiento.
/// </summary>
public sealed record DeleteWorkoutLogCommand(Guid Id) : IRequest;
