using MediatR;

namespace GymGo.Application.WorkoutLogs.Commands.UpdateWorkoutLog;

/// <summary>
/// Actualiza el título y las notas de una sesión de entrenamiento (solo en Draft).
/// </summary>
public sealed record UpdateWorkoutLogCommand(
    Guid Id,
    string? Title,
    string? Notes
) : IRequest;
