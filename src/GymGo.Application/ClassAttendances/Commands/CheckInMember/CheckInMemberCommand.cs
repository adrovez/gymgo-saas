using GymGo.Domain.ClassAttendances;
using MediatR;

namespace GymGo.Application.ClassAttendances.Commands.CheckInMember;

/// <summary>
/// Registra el check-in de un socio a una sesión concreta de clase.
/// </summary>
/// <param name="MemberId">Id del socio que asiste.</param>
/// <param name="ClassScheduleId">Id del horario semanal (ClassSchedule).</param>
/// <param name="SessionDate">Fecha de la sesión. Si es null, se usa la fecha UTC actual.</param>
/// <param name="CheckInMethod">Manual (default) o QR.</param>
/// <param name="Notes">Observaciones opcionales de la recepcionista.</param>
public sealed record CheckInMemberCommand(
    Guid MemberId,
    Guid ClassScheduleId,
    DateOnly? SessionDate,
    CheckInMethod CheckInMethod = CheckInMethod.Manual,
    string? Notes = null
) : IRequest<Guid>;
