namespace GymGo.Application.GymEntries.DTOs;

/// <summary>
/// Datos de un registro de ingreso al gimnasio.
/// </summary>
public sealed record GymEntryDto(
    Guid Id,
    Guid MemberId,
    string MemberFullName,
    Guid MembershipAssignmentId,
    DateOnly EntryDate,
    DateTime EnteredAtUtc,
    string Method,
    string? Notes,
    DateTime CreatedAtUtc
);
