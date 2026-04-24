using GymGo.Domain.Members;

namespace GymGo.Application.Members.DTOs;

/// <summary>
/// Representación resumida de un socio para listados (GET /members).
/// Omite campos de contacto de emergencia y notas para reducir payload.
/// </summary>
public sealed record MemberSummaryDto(
    Guid Id,
    string Rut,
    string FullName,
    string? Email,
    string? Phone,
    MemberStatus Status,
    string StatusLabel,
    DateOnly RegistrationDate
);
