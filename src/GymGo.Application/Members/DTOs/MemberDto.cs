using GymGo.Domain.Members;

namespace GymGo.Application.Members.DTOs;

/// <summary>
/// Representación completa de un socio para respuestas de GET /members/{id}.
/// </summary>
public sealed record MemberDto(
    Guid Id,
    Guid TenantId,
    string Rut,
    string FirstName,
    string LastName,
    string FullName,
    DateOnly BirthDate,
    int Age,
    Gender Gender,
    string GenderLabel,
    string? Email,
    string? Phone,
    string? Address,
    string? EmergencyContactName,
    string? EmergencyContactPhone,
    MemberStatus Status,
    string StatusLabel,
    DateOnly RegistrationDate,
    string? Notes,
    DateTime CreatedAtUtc,
    string? CreatedBy,
    DateTime? ModifiedAtUtc,
    string? ModifiedBy
);
