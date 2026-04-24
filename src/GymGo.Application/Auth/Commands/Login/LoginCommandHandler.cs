using GymGo.Application.Auth.DTOs;
using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Exceptions;
using GymGo.Domain.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.Auth.Commands.Login;

/// <summary>
/// Handler para <see cref="LoginCommand"/>.
///
/// Flujo:
///   1. Buscar usuario por email. Si el usuario es no-PlatformAdmin,
///      el HasQueryFilter de EF Core filtra automáticamente por el
///      tenant resuelto desde X-Tenant-Id (via ICurrentTenant).
///   2. Verificar contraseña con BCrypt.
///   3. Verificar que el usuario y el tenant estén activos.
///   4. Registrar el login y emitir el JWT.
///
/// Seguridad: todos los errores de credenciales retornan el mismo mensaje
/// genérico para no revelar si el email existe o no (credenciales inválidas).
/// </summary>
public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponseDto>
{
    private const string InvalidCredentialsCode = "AUTH_INVALID_CREDENTIALS";
    private const string InvalidCredentialsMessage = "Email o contraseña incorrectos.";

    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtGenerator;

    public LoginCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtGenerator)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtGenerator = jwtGenerator;
    }

    public async Task<LoginResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var emailNormalized = request.Email.Trim().ToLowerInvariant();

        // ── 1. Buscar usuario por email ──────────────────────────────────────
        // HasQueryFilter en Users aplica:
        //   (!HasTenant || u.TenantId == CurrentTenantId) && !u.IsDeleted
        // → PlatformAdmin (sin header) no tiene filtro de tenant, lo encuentra igual.
        // → GymAdmin/Receptionist/Instructor sólo se encuentran si su TenantId
        //   coincide con el X-Tenant-Id enviado en el header.
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == emailNormalized, cancellationToken);

        if (user is null)
            throw new BusinessRuleViolationException(InvalidCredentialsCode, InvalidCredentialsMessage);

        // ── 2. Verificar contraseña ──────────────────────────────────────────
        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new BusinessRuleViolationException(InvalidCredentialsCode, InvalidCredentialsMessage);

        // ── 3. Verificar que el usuario esté activo ──────────────────────────
        if (!user.IsActive)
            throw new BusinessRuleViolationException(
                "AUTH_USER_INACTIVE",
                "La cuenta de usuario está desactivada. Contacte al administrador.");

        // ── 4. Verificar tenant activo (sólo si el usuario tiene tenant) ─────
        if (user.Role != UserRole.PlatformAdmin && user.TenantId != Guid.Empty)
        {
            var tenantActive = await _context.Tenants
                .AnyAsync(t => t.Id == user.TenantId && t.IsActive, cancellationToken);

            if (!tenantActive)
                throw new BusinessRuleViolationException(
                    "AUTH_TENANT_INACTIVE",
                    "El gimnasio no está activo. Contacte al administrador de la plataforma.");
        }

        // ── 5. Registrar login y emitir JWT ──────────────────────────────────
        user.RegisterLogin();
        await _context.SaveChangesAsync(cancellationToken);

        var token = _jwtGenerator.Generate(user, out var expiresAtUtc);

        return new LoginResponseDto(
            Token:       token,
            ExpiresAtUtc: expiresAtUtc,
            UserId:      user.Id,
            FullName:    user.FullName,
            Email:       user.Email,
            Role:        user.Role,
            TenantId:    user.TenantId);
    }
}
