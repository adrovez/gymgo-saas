using GymGo.Domain.Common;
using GymGo.Domain.Exceptions;

namespace GymGo.Domain.Members;

/// <summary>
/// Socio del gimnasio. Entidad central del módulo de membresías.
/// Cada Member pertenece a un único Tenant (gimnasio) y puede o no
/// tener una cuenta de usuario (<see cref="Users.User"/>) asociada
/// para acceso a la app móvil.
///
/// Invariantes del dominio:
/// - El RUT debe ser válido según el algoritmo módulo 11 chileno.
/// - El email, si se provee, debe contener '@'.
/// - Nombre y apellido son obligatorios.
/// - El TenantId es inmutable una vez creado el socio.
/// - El estado sólo cambia a través de métodos explícitos (Activate, Suspend, MarkAsDelinquent).
/// </summary>
public sealed class Member : AggregateRoot, IAuditable, ITenantScoped, ISoftDeletable
{
    // ── Tenant ────────────────────────────────────────────────────────────
    /// <inheritdoc/>
    public Guid TenantId { get; set; }

    // ── Identificación ────────────────────────────────────────────────────
    /// <summary>RUT chileno normalizado (sin puntos, con guión). Ej: "12345678-9".</summary>
    public string Rut { get; private set; } = default!;

    /// <summary>Nombre(s) del socio.</summary>
    public string FirstName { get; private set; } = default!;

    /// <summary>Apellido(s) del socio.</summary>
    public string LastName { get; private set; } = default!;

    /// <summary>Fecha de nacimiento. Sólo fecha, sin hora.</summary>
    public DateOnly BirthDate { get; private set; }

    /// <summary>Género del socio (opcional, para estadísticas).</summary>
    public Gender Gender { get; private set; }

    // ── Contacto ─────────────────────────────────────────────────────────
    /// <summary>Email de contacto del socio (opcional). Normalizado a minúsculas.</summary>
    public string? Email { get; private set; }

    /// <summary>Número de celular con código de país (opcional). Ej: "+56912345678".</summary>
    public string? Phone { get; private set; }

    /// <summary>Dirección completa del domicilio (opcional).</summary>
    public string? Address { get; private set; }

    // ── Contacto de emergencia ────────────────────────────────────────────
    /// <summary>Nombre del contacto de emergencia (opcional).</summary>
    public string? EmergencyContactName { get; private set; }

    /// <summary>Teléfono del contacto de emergencia (opcional).</summary>
    public string? EmergencyContactPhone { get; private set; }

    // ── Estado y membresía ────────────────────────────────────────────────
    /// <summary>Estado operacional del socio.</summary>
    public MemberStatus Status { get; private set; }

    /// <summary>
    /// Fecha en que el socio fue dado de alta en el gimnasio.
    /// Puede diferir de <see cref="IAuditable.CreatedAtUtc"/> (que es técnico).
    /// </summary>
    public DateOnly RegistrationDate { get; private set; }

    // ── Observaciones ─────────────────────────────────────────────────────
    /// <summary>Notas internas del staff sobre el socio (opcional, max 1000 chars).</summary>
    public string? Notes { get; private set; }

    // ── IAuditable ────────────────────────────────────────────────────────
    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedAtUtc { get; set; }
    public string? ModifiedBy { get; set; }

    // ── ISoftDeletable ────────────────────────────────────────────────────
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public string? DeletedBy { get; set; }

    // ── Constructor privado para EF Core ──────────────────────────────────
    private Member() { }

    private Member(
        Guid id,
        Guid tenantId,
        string rut,
        string firstName,
        string lastName,
        DateOnly birthDate,
        Gender gender,
        string? email,
        string? phone,
        string? address,
        string? emergencyContactName,
        string? emergencyContactPhone,
        DateOnly registrationDate,
        string? notes)
        : base(id)
    {
        TenantId = tenantId;
        Rut = rut;
        FirstName = firstName;
        LastName = lastName;
        BirthDate = birthDate;
        Gender = gender;
        Email = email;
        Phone = phone;
        Address = address;
        EmergencyContactName = emergencyContactName;
        EmergencyContactPhone = emergencyContactPhone;
        Status = MemberStatus.Active;
        RegistrationDate = registrationDate;
        Notes = notes;
    }

    // ── Factory ───────────────────────────────────────────────────────────

    /// <summary>
    /// Crea un nuevo socio aplicando todas las reglas de negocio del dominio.
    /// </summary>
    /// <param name="tenantId">Gimnasio al que pertenece el socio.</param>
    /// <param name="rut">RUT chileno. Se acepta con o sin puntos y guión.</param>
    /// <param name="firstName">Nombre(s) del socio.</param>
    /// <param name="lastName">Apellido(s) del socio.</param>
    /// <param name="birthDate">Fecha de nacimiento.</param>
    /// <param name="gender">Género (opcional, default NotSpecified).</param>
    /// <param name="email">Email de contacto (opcional).</param>
    /// <param name="phone">Celular con código de país (opcional).</param>
    /// <param name="address">Dirección (opcional).</param>
    /// <param name="emergencyContactName">Nombre contacto emergencia (opcional).</param>
    /// <param name="emergencyContactPhone">Teléfono contacto emergencia (opcional).</param>
    /// <param name="registrationDate">Fecha de alta en el gimnasio. Si es null, se usa la fecha de hoy.</param>
    /// <param name="notes">Observaciones internas (opcional).</param>
    public static Member Create(
        Guid tenantId,
        string rut,
        string firstName,
        string lastName,
        DateOnly birthDate,
        Gender gender = Gender.NotSpecified,
        string? email = null,
        string? phone = null,
        string? address = null,
        string? emergencyContactName = null,
        string? emergencyContactPhone = null,
        DateOnly? registrationDate = null,
        string? notes = null)
    {
        // ── Tenant ────────────────────────────────────────────────
        if (tenantId == Guid.Empty)
            throw new BusinessRuleViolationException(
                "MEMBER_TENANT_REQUIRED",
                "El socio debe pertenecer a un gimnasio (TenantId requerido).");

        // ── RUT ───────────────────────────────────────────────────
        if (string.IsNullOrWhiteSpace(rut))
            throw new BusinessRuleViolationException(
                "MEMBER_RUT_REQUIRED",
                "El RUT es obligatorio.");

        var normalizedRut = NormalizeRut(rut);
        if (!IsValidRut(normalizedRut))
            throw new BusinessRuleViolationException(
                "MEMBER_RUT_INVALID",
                $"El RUT '{rut}' no es válido.");

        // ── Nombre y apellido ─────────────────────────────────────
        if (string.IsNullOrWhiteSpace(firstName))
            throw new BusinessRuleViolationException(
                "MEMBER_FIRSTNAME_REQUIRED",
                "El nombre del socio es obligatorio.");

        if (string.IsNullOrWhiteSpace(lastName))
            throw new BusinessRuleViolationException(
                "MEMBER_LASTNAME_REQUIRED",
                "El apellido del socio es obligatorio.");

        // ── Fecha de nacimiento ───────────────────────────────────
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (birthDate >= today)
            throw new BusinessRuleViolationException(
                "MEMBER_BIRTHDATE_INVALID",
                "La fecha de nacimiento debe ser anterior a la fecha actual.");

        if (birthDate < new DateOnly(1900, 1, 1))
            throw new BusinessRuleViolationException(
                "MEMBER_BIRTHDATE_TOO_OLD",
                "La fecha de nacimiento no es válida.");

        // ── Email (si se proporciona) ──────────────────────────────
        if (!string.IsNullOrWhiteSpace(email) && !email.Contains('@'))
            throw new BusinessRuleViolationException(
                "MEMBER_EMAIL_INVALID",
                "El email no tiene un formato válido.");

        // ── Notas (límite de largo) ───────────────────────────────
        if (notes is not null && notes.Length > 1000)
            throw new BusinessRuleViolationException(
                "MEMBER_NOTES_TOO_LONG",
                "Las observaciones no pueden superar los 1000 caracteres.");

        return new Member(
            Guid.NewGuid(),
            tenantId,
            normalizedRut,
            firstName.Trim(),
            lastName.Trim(),
            birthDate,
            gender,
            string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant(),
            string.IsNullOrWhiteSpace(phone) ? null : phone.Trim(),
            string.IsNullOrWhiteSpace(address) ? null : address.Trim(),
            string.IsNullOrWhiteSpace(emergencyContactName) ? null : emergencyContactName.Trim(),
            string.IsNullOrWhiteSpace(emergencyContactPhone) ? null : emergencyContactPhone.Trim(),
            registrationDate ?? today,
            string.IsNullOrWhiteSpace(notes) ? null : notes.Trim());
    }

    // ── Comportamiento ────────────────────────────────────────────────────

    /// <summary>Activa al socio (Status → Active).</summary>
    public void Activate() => Status = MemberStatus.Active;

    /// <summary>Suspende al socio manualmente (Status → Suspended).</summary>
    public void Suspend() => Status = MemberStatus.Suspended;

    /// <summary>Marca al socio como moroso (Status → Delinquent).</summary>
    public void MarkAsDelinquent() => Status = MemberStatus.Delinquent;

    /// <summary>Actualiza los datos de contacto y personales del socio.</summary>
    public void Update(
        string firstName,
        string lastName,
        DateOnly birthDate,
        Gender gender,
        string? email,
        string? phone,
        string? address,
        string? emergencyContactName,
        string? emergencyContactPhone,
        string? notes)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new BusinessRuleViolationException(
                "MEMBER_FIRSTNAME_REQUIRED",
                "El nombre del socio es obligatorio.");

        if (string.IsNullOrWhiteSpace(lastName))
            throw new BusinessRuleViolationException(
                "MEMBER_LASTNAME_REQUIRED",
                "El apellido del socio es obligatorio.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (birthDate >= today)
            throw new BusinessRuleViolationException(
                "MEMBER_BIRTHDATE_INVALID",
                "La fecha de nacimiento debe ser anterior a la fecha actual.");

        if (!string.IsNullOrWhiteSpace(email) && !email.Contains('@'))
            throw new BusinessRuleViolationException(
                "MEMBER_EMAIL_INVALID",
                "El email no tiene un formato válido.");

        if (notes is not null && notes.Length > 1000)
            throw new BusinessRuleViolationException(
                "MEMBER_NOTES_TOO_LONG",
                "Las observaciones no pueden superar los 1000 caracteres.");

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        BirthDate = birthDate;
        Gender = gender;
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();
        Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
        Address = string.IsNullOrWhiteSpace(address) ? null : address.Trim();
        EmergencyContactName = string.IsNullOrWhiteSpace(emergencyContactName) ? null : emergencyContactName.Trim();
        EmergencyContactPhone = string.IsNullOrWhiteSpace(emergencyContactPhone) ? null : emergencyContactPhone.Trim();
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }

    // ── Lógica de RUT chileno ─────────────────────────────────────────────

    /// <summary>
    /// Normaliza el RUT a formato "XXXXXXXX-D" (sin puntos, con guión, mayúscula).
    /// Acepta: "12.345.678-9", "12345678-9", "123456789".
    /// </summary>
    private static string NormalizeRut(string rut)
    {
        // Quitar puntos y espacios
        var clean = rut.Replace(".", "").Replace(" ", "").ToUpperInvariant();

        // Si no tiene guión y tiene al menos 2 chars, insertar guión antes del último
        if (!clean.Contains('-') && clean.Length >= 2)
            clean = clean[..^1] + "-" + clean[^1];

        return clean;
    }

    /// <summary>
    /// Valida el RUT chileno usando el algoritmo módulo 11.
    /// El RUT debe estar normalizado ("XXXXXXXX-D") antes de llamar este método.
    /// </summary>
    private static bool IsValidRut(string normalizedRut)
    {
        var parts = normalizedRut.Split('-');
        if (parts.Length != 2) return false;

        var body = parts[0];
        var verifier = parts[1];

        if (string.IsNullOrEmpty(body) || string.IsNullOrEmpty(verifier)) return false;
        if (!long.TryParse(body, out var number) || number <= 0) return false;

        var expectedVerifier = ComputeRutVerifier(number);
        return string.Equals(verifier, expectedVerifier, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Calcula el dígito verificador de un RUT usando módulo 11.
    /// Retorna "0"-"9" o "K".
    /// </summary>
    private static string ComputeRutVerifier(long rutNumber)
    {
        var sum = 0;
        var multiplier = 2;

        while (rutNumber > 0)
        {
            sum += (int)(rutNumber % 10) * multiplier;
            rutNumber /= 10;
            multiplier = multiplier == 7 ? 2 : multiplier + 1;
        }

        var remainder = 11 - (sum % 11);
        return remainder switch
        {
            11 => "0",
            10 => "K",
            _ => remainder.ToString()
        };
    }
}
