using GymGo.Domain.Common;
using GymGo.Domain.Exceptions;
using GymGo.Domain.Maintenance;

namespace GymGo.Domain.Equipments;

/// <summary>
/// Máquina o equipo físico del gimnasio.
///
/// Invariantes:
/// - Name es obligatorio y tiene máx. 100 caracteres.
/// - Solo se puede desactivar una máquina activa y reactivar una inactiva.
/// </summary>
public sealed class Equipment : AggregateRoot, IAuditable, ITenantScoped, ISoftDeletable
{
    // ── Tenant ────────────────────────────────────────────────────────────
    public Guid TenantId { get; set; }

    // ── Datos ─────────────────────────────────────────────────────────────
    public string Name { get; private set; } = string.Empty;
    public string? Brand { get; private set; }
    public string? Model { get; private set; }
    public string? SerialNumber { get; private set; }
    public DateOnly? PurchaseDate { get; private set; }
    public string? ImageUrl { get; private set; }

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

    // ── Navegación ────────────────────────────────────────────────────────
    private readonly List<MaintenanceRecord> _maintenanceRecords = [];
    public IReadOnlyCollection<MaintenanceRecord> MaintenanceRecords => _maintenanceRecords.AsReadOnly();

    // ── Constructor privado para EF Core ──────────────────────────────────
    private Equipment() { }

    private Equipment(
        Guid id, Guid tenantId, string name, string? brand, string? model,
        string? serialNumber, DateOnly? purchaseDate, string? imageUrl)
        : base(id)
    {
        TenantId     = tenantId;
        Name         = name;
        Brand        = brand;
        Model        = model;
        SerialNumber = serialNumber;
        PurchaseDate = purchaseDate;
        ImageUrl     = imageUrl;
        IsActive     = true;
    }

    // ── Factory ───────────────────────────────────────────────────────────

    public static Equipment Create(
        Guid tenantId,
        string name,
        string? brand,
        string? model,
        string? serialNumber,
        DateOnly? purchaseDate,
        string? imageUrl)
    {
        Validate(tenantId, name);

        return new Equipment(
            Guid.NewGuid(), tenantId,
            name.Trim(),
            string.IsNullOrWhiteSpace(brand)        ? null : brand.Trim(),
            string.IsNullOrWhiteSpace(model)        ? null : model.Trim(),
            string.IsNullOrWhiteSpace(serialNumber) ? null : serialNumber.Trim(),
            purchaseDate,
            string.IsNullOrWhiteSpace(imageUrl)     ? null : imageUrl.Trim());
    }

    // ── Comportamiento ────────────────────────────────────────────────────

    public void Update(
        string name,
        string? brand,
        string? model,
        string? serialNumber,
        DateOnly? purchaseDate,
        string? imageUrl)
    {
        Validate(TenantId, name);

        Name         = name.Trim();
        Brand        = string.IsNullOrWhiteSpace(brand)        ? null : brand.Trim();
        Model        = string.IsNullOrWhiteSpace(model)        ? null : model.Trim();
        SerialNumber = string.IsNullOrWhiteSpace(serialNumber) ? null : serialNumber.Trim();
        PurchaseDate = purchaseDate;
        ImageUrl     = string.IsNullOrWhiteSpace(imageUrl)     ? null : imageUrl.Trim();
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new BusinessRuleViolationException(
                "EQUIPMENT_ALREADY_INACTIVE", "La máquina ya está inactiva.");

        IsActive = false;
    }

    public void Reactivate()
    {
        if (IsActive)
            throw new BusinessRuleViolationException(
                "EQUIPMENT_ALREADY_ACTIVE", "La máquina ya está activa.");

        IsActive = true;
    }

    // ── Helpers privados ─────────────────────────────────────────────────

    private static void Validate(Guid tenantId, string name)
    {
        if (tenantId == Guid.Empty)
            throw new BusinessRuleViolationException(
                "EQUIPMENT_TENANT_REQUIRED", "El tenant es obligatorio.");

        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessRuleViolationException(
                "EQUIPMENT_NAME_REQUIRED", "El nombre de la máquina es obligatorio.");

        if (name.Length > 100)
            throw new BusinessRuleViolationException(
                "EQUIPMENT_NAME_TOO_LONG", "El nombre no puede superar los 100 caracteres.");
    }
}
