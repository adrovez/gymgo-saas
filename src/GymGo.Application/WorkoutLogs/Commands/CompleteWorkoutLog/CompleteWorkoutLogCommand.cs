using MediatR;

namespace GymGo.Application.WorkoutLogs.Commands.CompleteWorkoutLog;

/// <summary>
/// Marca una sesión de entrenamiento como completada (Draft → Completed).
/// Operación irreversible. La sesión debe tener al menos un ejercicio.
/// </summary>
public sealed record CompleteWorkoutLogCommand(Guid Id) : IRequest;
