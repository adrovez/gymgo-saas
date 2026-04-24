using GymGo.Domain.Members;
using MediatR;

namespace GymGo.Application.Members.Commands.UpdateMember;

/// <summary>
/// Comando para actualizar los datos personales y de contacto de un socio.
/// No modifica el RUT (inmutable) ni el estado (usa ChangeMemberStatusCommand).
/// </summary>
public sealed record UpdateMemberCommand(
    Guid MemberId,
    string FirstName,
    string LastName,
    DateOnly BirthDate,
    Gender Gender,
    string? Email,
    string? Phone,
    string? Address,
    string? EmergencyContactName,
    string? EmergencyContactPhone,
    string? Notes
) : IRequest;
