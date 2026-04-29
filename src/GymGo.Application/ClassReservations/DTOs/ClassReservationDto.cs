using GymGo.Domain.ClassReservations;

namespace GymGo.Application.ClassReservations.DTOs;

/// <summary>
/// Datos completos de una reserva de clase.
/// </summary>
public sealed record ClassReservationDto(
    Guid Id,
    Guid MemberId,
    string MemberFullName,
    Guid ClassScheduleId,
    DateOnly SessionDate,
    DateTime ReservedAtUtc,
    ReservationStatus Status,
    string? Notes,
    DateTime? CancelledAtUtc,
    string? CancelledBy,
    string? CancelReason
);
