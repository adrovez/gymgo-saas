using GymGo.Domain.Common;
using GymGo.Domain.Exceptions;

namespace GymGo.Domain.Tenants;

/// <summary>
/// Tenant = un gimnasio cliente del SaaS. Es el límite superior
/// de aislamiento de datos.
/// </summary>
public sealed class Tenant : AggregateRoot, IAuditable
{
    public string Name { get; private set; } = default!;
    public string Slug { get; private set; } = default!;
    public string? ContactEmail { get; private set; }
    public string? ContactPhone { get; private set; }
    public bool IsActive { get; private set; }

    // IAuditable
    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedAtUtc { get; set; }
    public string? ModifiedBy { get; set; }

    private Tenant() { } // EF Core

    private Tenant(Guid id, string name, string slug, string? contactEmail, string? contactPhone)
        : base(id)
    {
        Name = name;
        Slug = slug;
        ContactEmail = contactEmail;
        ContactPhone = contactPhone;
        IsActive = true;
    }

    public static Tenant Create(string name, string slug, string? contactEmail = null, string? contactPhone = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessRuleViolationException("TENANT_NAME_REQUIRED", "El nombre del tenant es obligatorio.");
        if (string.IsNullOrWhiteSpace(slug))
            throw new BusinessRuleViolationException("TENANT_SLUG_REQUIRED", "El slug del tenant es obligatorio.");
        if (slug.Length > 60)
            throw new BusinessRuleViolationException("TENANT_SLUG_TOO_LONG", "El slug no puede superar 60 caracteres.");

        return new Tenant(Guid.NewGuid(), name.Trim(), slug.Trim().ToLowerInvariant(), contactEmail, contactPhone);
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;

    public void UpdateContact(string? email, string? phone)
    {
        ContactEmail = email;
        ContactPhone = phone;
    }
}
