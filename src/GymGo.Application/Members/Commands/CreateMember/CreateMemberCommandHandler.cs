using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Exceptions;
using GymGo.Domain.Members;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.Members.Commands.CreateMember;

/// <summary>
/// Handler para <see cref="CreateMemberCommand"/>.
/// Retorna el <see cref="Guid"/> del nuevo socio creado.
/// </summary>
public sealed class CreateMemberCommandHandler : IRequestHandler<CreateMemberCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenant _currentTenant;

    public CreateMemberCommandHandler(IApplicationDbContext context, ICurrentTenant currentTenant)
    {
        _context = context;
        _currentTenant = currentTenant;
    }

    public async Task<Guid> Handle(CreateMemberCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _currentTenant.TenantId
            ?? throw new UnauthorizedAccessException("No se pudo determinar el tenant actual.");

        // Unicidad de RUT dentro del tenant (normalizado para comparación consistente)
        var rutNormalized = request.Rut
            .Replace(".", "")
            .Replace(" ", "")
            .ToUpperInvariant();

        // Insertar guión si no tiene, para comparar con el formato guardado
        if (!rutNormalized.Contains('-') && rutNormalized.Length >= 2)
            rutNormalized = rutNormalized[..^1] + "-" + rutNormalized[^1];

        var exists = await _context.Members
            .AnyAsync(m => m.Rut == rutNormalized, cancellationToken);

        if (exists)
            throw new BusinessRuleViolationException(
                "MEMBER_RUT_DUPLICATE",
                $"Ya existe un socio con el RUT '{request.Rut}' en este gimnasio.");

        var member = Member.Create(
            tenantId:               tenantId,
            rut:                    request.Rut,
            firstName:              request.FirstName,
            lastName:               request.LastName,
            birthDate:              request.BirthDate,
            gender:                 request.Gender,
            email:                  request.Email,
            phone:                  request.Phone,
            address:                request.Address,
            emergencyContactName:   request.EmergencyContactName,
            emergencyContactPhone:  request.EmergencyContactPhone,
            registrationDate:       request.RegistrationDate,
            notes:                  request.Notes);

        _context.Members.Add(member);
        await _context.SaveChangesAsync(cancellationToken);

        return member.Id;
    }
}
