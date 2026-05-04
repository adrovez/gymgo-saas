using MediatR;

namespace GymGo.Application.GymEntries.Commands.RegisterGymExit;

/// <summary>
/// Registra la salida de un socio del gimnasio.
/// Actualiza el campo ExitedAtUtc del registro de ingreso correspondiente.
/// </summary>
public sealed record RegisterGymExitCommand(Guid EntryId) : IRequest;
