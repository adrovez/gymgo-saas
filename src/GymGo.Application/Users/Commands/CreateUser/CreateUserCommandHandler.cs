using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Exceptions;
using GymGo.Domain.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.Users.Commands.CreateUser;

/// <summary>
/// Handler para <see cref="CreateUserCommand"/>.
/// Retorna el <see cref="Guid"/> del nuevo usuario creado.
/// </summary>
public sealed class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenant _currentTenant;
    private readonly IPasswordHasher _passwordHasher;

    public CreateUserCommandHandler(
        IApplicationDbContext context,
        ICurrentTenant currentTenant,
        IPasswordHasher passwordHasher)
    {
        _context = context;
        _currentTenant = currentTenant;
        _passwordHasher = passwordHasher;
    }

    public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _currentTenant.TenantId
            ?? throw new UnauthorizedAccessException("No se pudo determinar el tenant actual.");

        var emailNormalized = request.Email.Trim().ToLowerInvariant();

        // Unicidad de email dentro del tenant
        var exists = await _context.Users
            .AnyAsync(u => u.Email == emailNormalized, cancellationToken);

        if (exists)
            throw new BusinessRuleViolationException(
                "USER_EMAIL_DUPLICATE",
                $"Ya existe un usuario con el email '{request.Email}' en este gimnasio.");

        var passwordHash = _passwordHasher.Hash(request.Password);

        var user = User.Create(
            tenantId:     tenantId,
            email:        emailNormalized,
            passwordHash: passwordHash,
            fullName:     request.FullName,
            role:         request.Role);

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}
