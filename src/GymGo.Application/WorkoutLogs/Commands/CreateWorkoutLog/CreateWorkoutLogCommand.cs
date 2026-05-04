using MediatR;

namespace GymGo.Application.WorkoutLogs.Commands.CreateWorkoutLog;

/// <summary>
/// Crea una nueva sesión de entrenamiento (WorkoutLog) para un socio.
/// </summary>
/// <param name="MemberId">Id del socio que realizó el entrenamiento.</param>
/// <param name="Date">Fecha de la sesión. Si es null se usa la fecha actual UTC.</param>
/// <param name="Title">Título opcional de la rutina (ej: "Día A – Push").</param>
/// <param name="Notes">Observaciones generales de la sesión (opcional).</param>
public sealed record CreateWorkoutLogCommand(
    Guid MemberId,
    DateOnly? Date,
    string? Title,
    string? Notes
) : IRequest<Guid>;
