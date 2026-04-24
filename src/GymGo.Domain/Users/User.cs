using GymGo.Domain.Common;
using GymGo.Domain.Exceptions;

namespace GymGo.Domain.Users;

/// <summary>
/// Usuario del sistema. PlatformAdmin no tiene tenant; el resto sí.
/// El TenantId se asigna en construcción y es inmutable.
/// </summary>
public sealed class User : AggregateRoot, IAuditable, ITenantScoped, ISoftDeletable
{
    public Guid TenantId { get; set; }
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public string FullName { get; private set; } = default!;
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? LastLoginUtc { get; private set; }

    // IAuditable
    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedAtUtc { get; set; }
    public string? ModifiedBy { get; set; }

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public string? DeletedBy { get; set; }

    private User() { } // EF Core

    private User(Guid id, Guid tenantId, string email, string passwordHash, string fullName, UserRole role)
        : base(id)
    {
        TenantId = tenantId;
        Email = email;
        PasswordHash = passwordHash;
        FullName = fullName;
        Role = role;
        IsActive = true;
    }

    public static User Create(Guid tenantId, string email, string passwordHash, string fullName, UserRole role)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new BusinessRuleViolationException("USER_EMAIL_REQUIRED", "El email es obligatorio.");
        if (!email.Contains('@'))
            throw new BusinessRuleViolationException("USER_EMAIL_INVALID", "El email no tiene formato válido.");
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new BusinessRuleViolationException("USER_PASSWORD_REQUIRED", "La contraseña es obligatoria.");
        if (string.IsNullOrWhiteSpace(fullName))
            throw new BusinessRuleViolationException("USER_FULLNAME_REQUIRED", "El nombre completo es obligatorio.");
        if (role != UserRole.PlatformAdmin && tenantId == Guid.Empty)
            throw new BusinessRuleViolationException("USER_TENANT_REQUIRED", "Todo usuario no-PlatformAdmin debe pertenecer a un tenant.");

        return new User(Guid.NewGuid(), tenantId, email.Trim().ToLowerInvariant(), passwordHash, fullName.Trim(), role);
    }

    public void ChangePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new BusinessRuleViolationException("USER_PASSWORD_REQUIRED", "La contraseña es obligatoria.");
        PasswordHash = newPasswordHash;
    }

    public void RegisterLogin() => LastLoginUtc = DateTime.UtcNow;

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
