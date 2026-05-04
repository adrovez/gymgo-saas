using MediatR;

namespace GymGo.Application.WorkoutLogs.Commands.RemoveExercise;

/// <summary>
/// Elimina un ejercicio de una sesión de entrenamiento (solo en Draft).
/// </summary>
public sealed record RemoveExerciseCommand(Guid WorkoutLogId, Guid ExerciseId) : IRequest;
