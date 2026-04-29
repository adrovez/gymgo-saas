using GymGo.Domain.ClassAttendances;

namespace GymGo.Application.ClassAttendances.DTOs;

/// <summary>
/// DTO de un registro de asistencia devuelto al frontend.
/// </summary>
public sealed record ClassAttendanceDto(
    Guid Id,
    Guid MemberId,
    string MemberFullName,
    Guid ClassScheduleId,
    DateOnly SessionDate,
    DateTime CheckedInAtUtc,
    CheckInMethod CheckInMethod,
    string? Notes
);
