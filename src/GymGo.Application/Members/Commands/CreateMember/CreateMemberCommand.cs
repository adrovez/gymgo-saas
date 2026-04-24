using GymGo.Domain.Members;
using MediatR;

namespace GymGo.Application.Members.Commands.CreateMember;

/// <summary>
/// Comando para dar de alta a un nuevo socio en el gimnasio.
/// El TenantId no se incluye aquí: lo inyecta el handler desde ICurrentTenant.
/// </summary>
public sealed record CreateMemberCommand(
    string Rut,
    string FirstName,
    string LastName,
    DateOnly BirthDate,
    Gender Gender,
    string? Email,
    string? Phone,
    string? Address,
    string? EmergencyContactName,
    string? EmergencyContactPhone,
    DateOnly? RegistrationDate,
    string? Notes
) : IRequest<Guid>;
