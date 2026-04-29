using GymGo.Domain.Common;
using GymGo.Domain.Exceptions;

namespace GymGo.Domain.GymClasses;

/// <summary>
/// Tipo de clase que ofrece el gimnasio (ej. Yoga, Spinning, Box Funcional).
/// Es el catálogo/plantilla del que se generan horarios semanales.
///
/// Invariantes:
/// - Name no puede ser vacío y tiene máx. 100 caracteres.
/// - DurationMinutes y MaxCapacity deben ser positivos.
/// - Solo se puede desactivar un tipo activo y reactivar uno inactivo.
/// </summary>
public sealed class GymClass : AggregateRoot, IAuditable, ITenantScoped, ISoftDeletable
{
    // ── Tenant ────────────────────────────────────────────────────────────
    public Guid TenantId { get; set; }

    // ── Datos ─────────────────────────────────────────────────────────────
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public ClassCategory Category { get; private set; }

    /// <summary>Color hex para el calendario (ej. "#3B82F6"). Opcional.</summary>
    public string? Color { get; private set; }

    /// <summary>Duración estándar en minutos.</summary>
    public int DurationMinutes { get; private set; }

    /// <summary>Capacidad máxima estándar de la clase.</summary>
    public int MaxCapacity { get; private set; }

    // ── Estado ────────────────────────────────────────────────────────────
    public bool IsActive { get; private set; }

    // ── IAuditable ────────────────────────────────────────────────────────
    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedAtUtc { get; set; }
    public string? ModifiedBy { get; set; }

    // ── ISoftDeletable ────────────────────────────────────────────────────
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public string? DeletedBy { get; set; }

    // ── Navegación (solo lectura, cargada por EF) ─────────────────────────
    private readonly List<ClassSchedule> _schedules = [];
    public IReadOnlyCollection<ClassSchedule> Schedules => _schedules.AsReadOnly();

    // ── Constructor privado para EF Core ──────────────────────────────────
    private GymClass() { }

    private GymClass(
        Guid id, Guid tenantId, string name, string? description,
        ClassCategory category, string? color, int durationMinutes, int maxCapacity)
        : base(id)
    {
        TenantId        = tenantId;
        Name            = name;
        Description     = description;
        Category        = category;
        Color           = color;
        DurationMinutes = durationMinutes;
        MaxCapacity     = maxCapacity;
        IsActive        = true;
    }

    // ── Factory ───────────────────────────────────────────────────────────

    public static GymClass Create(
        Guid tenantId,
        string name,
        string? description,
        ClassCategory category,
        string? color,
        int durationMinutes,
        int maxCapacity)
    {
        Validate(tenantId, name, durationMinutes, maxCapacity);

        return new GymClass(
            Guid.NewGuid(), tenantId,
            name.Trim(),
            string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            category,
            NormalizeColor(color),
            durationMinutes,
            maxCapacity);
    }

    // ── Comportamiento ────────────────────────────────────────────────────

    public void Update(
        string name,
        string? description,
        ClassCategory category,
        string? color,
        int durationMinutes,
        int maxCapacity)
    {
        Validate(TenantId, name, durationMinutes, maxCapacity);

        Name            = name.Trim();
        Description     = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Category        = category;
        Color           = NormalizeColor(color);
        DurationMinutes = durationMinutes;
        MaxCapacity     = maxCapacity;
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new BusinessRuleViolationException(
                "GYMCLASS_ALREADY_INACTIVE", "La clase ya está inactiva.");

        IsActive = false;
    }

    public void Reactivate()
    {
        if (IsActive)
            throw new BusinessRuleViolationException(
                "GYMCLASS_ALREADY_ACTIVE", "La clase ya está activa.");

        IsActive = true;
    }

    // ── Helpers privados ─────────────────────────────────────────────────

    private static void Validate(Guid tenantId, string name, int durationMinutes, int maxCapacity)
    {
        if (tenantId == Guid.Empty)
            throw new BusinessRuleViolationException(
                "GYMCLASS_TENANT_REQUIRED", "El tenant es obligatorio.");

        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessRuleViolationException(
                "GYMCLASS_NAME_REQUIRED", "El nombre de la clase es obligatorio.");

        if (name.Length > 100)
            throw new BusinessRuleViolationException(
                "GYMCLASS_NAME_TOO_LONG", "El nombre no puede superar los 100 caracteres.");

        if (durationMinutes <= 0)
            throw new BusinessRuleViolationException(
                "GYMCLASS_DURATION_INVALID", "La duración debe ser mayor a cero.");

        if (maxCapacity <= 0)
            throw new BusinessRuleViolationException(
                "GYMCLASS_CAPACITY_INVALID", "La capacidad máxima debe ser mayor a cero.");
    }

    private static string? NormalizeColor(string? color)
    {
        if (string.IsNullOrWhiteSpace(color)) return null;
        var c = color.Trim();
        // Asegurar que empieza con #
        return c.StartsWith('#') ? c : $"#{c}";
    }
}
