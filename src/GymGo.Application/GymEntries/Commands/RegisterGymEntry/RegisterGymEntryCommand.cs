using GymGo.Domain.GymEntries;
using MediatR;

namespace GymGo.Application.GymEntries.Commands.RegisterGymEntry;

/// <summary>
/// Registra el ingreso de un socio al gimnasio validando membresía activa
/// y las restricciones de días y horario definidas en el plan.
/// </summary>
/// <param name="MemberId">Id del socio que ingresa.</param>
/// <param name="Method">Método de registro: Manual (default), QR o Badge.</param>
/// <param name="Notes">Observaciones opcionales de la recepcionista.</param>
public sealed record RegisterGymEntryCommand(
    Guid MemberId,
    GymEntryMethod Method = GymEntryMethod.Manual,
    string? Notes = null
) : IRequest<Guid>;
