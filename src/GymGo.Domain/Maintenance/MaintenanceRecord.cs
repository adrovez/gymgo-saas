using GymGo.Domain.Common;
using GymGo.Domain.Equipments;
using GymGo.Domain.Exceptions;

namespace GymGo.Domain.Maintenance;

/// <summary>
/// Registro de una mantención (preventiva o correctiva) para una máquina.
///
/// Invariantes:
/// - Solo se puede iniciar una mantención en estado Pending.
/// - Solo se puede completar una mantención en estado InProgress.
/// - No se puede cancelar una mantención ya Completada o Cancelada.
/// - Si el responsable es interno, ResponsibleUserId es obligatorio.
/// - Si el responsable es externo, ExternalProviderName es obligatorio.
/// </summary>
public sealed class MaintenanceRecord : AggregateRoot, IAuditable, ITenantScoped
{
    // ── Tenant ────────────────────────────────────────────────────────────
    public Guid TenantId { get; set; }

    // ── Relación con máquina ──────────────────────────────────────────────
    public Guid EquipmentId { get; private set; }

    // ── Tipo y estado ─────────────────────────────────────────────────────
    public MaintenanceType   Type   { get; private set; }
    public MaintenanceStatus Status { get; private set; }

    // ── Fechas ────────────────────────────────────────────────────────────
    /// <summary>Fecha programada de la mantención.</summary>
    public DateOnly ScheduledDate { get; private set; }

    /// <summary>Momento real en que se inició la mantención (UTC).</summary>
    public DateTime? StartedAtUtc { get; private set; }

    /// <summary>Momento real en que se completó o canceló (UTC).</summary>
    public DateTime? CompletedAtUtc { get; private set; }

    // ── Descripción ───────────────────────────────────────────────────────
    /// <summary>Descripción del trabajo a realizar / realizado.</summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>Observaciones adicionales, motivo de cancelación, etc.</summary>
    public string? Notes { get; private set; }

    /// <summary>Costo incurrido. Se registra al completar la mantención.</summary>
    public decimal? Cost { get; private set; }

    // ── Responsable ───────────────────────────────────────────────────────
    public ResponsibleType ResponsibleType { get; private set; }

    /// <summary>Id del usuario interno responsable (solo si ResponsibleType = Internal).</summary>
    public Guid? ResponsibleUserId { get; private set; }

    /// <summary>Nombre del proveedor externo (solo si ResponsibleType = External).</summary>
    public string? ExternalProviderName { get; private set; }

    /// <summary>Contacto del proveedor externo: teléfono o email.</summary>
    public string? ExternalProviderContact { get; private set; }

    // ── IAuditable ────────────────────────────────────────────────────────
    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedAtUtc { get; set; }
    public string? ModifiedBy { get; set; }

    // ── Navegación ────────────────────────────────────────────────────────
    public Equipment Equipment { get; private set; } = null!;

    // ── Constructor privado para EF Core ──────────────────────────────────
    private MaintenanceRecord() { }

    private MaintenanceRecord(
        Guid id, Guid tenantId, Guid equipmentId,
        MaintenanceType type, DateOnly scheduledDate, string description,
        ResponsibleType responsibleType, Guid? responsibleUserId,
        string? externalProviderName, string? externalProviderContact)
        : base(id)
    {
        TenantId                = tenantId;
        EquipmentId             = equipmentId;
        Type                    = type;
        Status                  = MaintenanceStatus.Pending;
        ScheduledDate           = scheduledDate;
        Description             = description;
        ResponsibleType         = responsibleType;
        ResponsibleUserId       = responsibleUserId;
        ExternalProviderName    = externalProviderName;
        ExternalProviderContact = externalProviderContact;
    }

    // ── Factory ───────────────────────────────────────────────────────────

    public static MaintenanceRecord Create(
        Guid tenantId,
        Guid equipmentId,
        MaintenanceType type,
        DateOnly scheduledDate,
        string description,
        ResponsibleType responsibleType,
        Guid? responsibleUserId,
        string? externalProviderName,
        string? externalProviderContact)
    {
        ValidateCreate(tenantId, equipmentId, description,
                       responsibleType, responsibleUserId, externalProviderName);

        return new MaintenanceRecord(
            Guid.NewGuid(), tenantId, equipmentId,
            type, scheduledDate,
            description.Trim(),
            responsibleType,
            responsibleUserId,
            string.IsNullOrWhiteSpace(externalProviderName)    ? null : externalProviderName.Trim(),
            string.IsNullOrWhiteSpace(externalProviderContact) ? null : externalProviderContact.Trim());
    }

    // ── Comportamiento ────────────────────────────────────────────────────

    /// <summary>
    /// Inicia la ejecución de la mantención (Pending → InProgress).
    /// </summary>
    public void Start()
    {
        if (Status != MaintenanceStatus.Pending)
            throw new BusinessRuleViolationException(
                "MAINTENANCE_CANNOT_START",
                "Solo se puede iniciar una mantención que esté en estado Pendiente.");

        Status       = MaintenanceStatus.InProgress;
        StartedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Marca la mantención como completada (InProgress → Completed).
    /// </summary>
    public void Complete(string? notes, decimal? cost)
    {
        if (Status != MaintenanceStatus.InProgress)
            throw new BusinessRuleViolationException(
                "MAINTENANCE_CANNOT_COMPLETE",
                "Solo se puede completar una mantención que esté en estado En Proceso.");

        if (cost.HasValue && cost.Value < 0)
            throw new BusinessRuleViolationException(
                "MAINTENANCE_COST_NEGATIVE", "El costo no puede ser negativo.");

        Status         = MaintenanceStatus.Completed;
        CompletedAtUtc = DateTime.UtcNow;
        Notes          = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        Cost           = cost;
    }

    /// <summary>
    /// Cancela la mantención (Pending o InProgress → Cancelled).
    /// </summary>
    public void Cancel(string? reason)
    {
        if (Status is MaintenanceStatus.Completed or MaintenanceStatus.Cancelled)
            throw new BusinessRuleViolationException(
                "MAINTENANCE_CANNOT_CANCEL",
                "No se puede cancelar una mantención que ya esté Completada o Cancelada.");

        Status         = MaintenanceStatus.Cancelled;
        CompletedAtUtc = DateTime.UtcNow;
        Notes          = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
    }

    // ── Helpers privados ─────────────────────────────────────────────────

    private static void ValidateCreate(
        Guid tenantId, Guid equipmentId, string description,
        ResponsibleType responsibleType, Guid? responsibleUserId, string? externalProviderName)
    {
        if (tenantId == Guid.Empty)
            throw new BusinessRuleViolationException(
                "MAINTENANCE_TENANT_REQUIRED", "El tenant es obligatorio.");

        if (equipmentId == Guid.Empty)
            throw new BusinessRuleViolationException(
                "MAINTENANCE_EQUIPMENT_REQUIRED", "La máquina es obligatoria.");

        if (string.IsNullOrWhiteSpace(description))
            throw new BusinessRuleViolationException(
                "MAINTENANCE_DESCRIPTION_REQUIRED", "La descripción es obligatoria.");

        if (description.Length > 500)
            throw new BusinessRuleViolationException(
                "MAINTENANCE_DESCRIPTION_TOO_LONG", "La descripción no puede superar los 500 caracteres.");

        if (responsibleType == ResponsibleType.Internal && responsibleUserId is null)
            throw new BusinessRuleViolationException(
                "MAINTENANCE_USER_REQUIRED",
                "Debe indicar el usuario responsable para una mantención interna.");

        if (responsibleType == ResponsibleType.External && string.IsNullOrWhiteSpace(externalProviderName))
            throw new BusinessRuleViolationException(
                "MAINTENANCE_PROVIDER_REQUIRED",
                "Debe indicar el nombre del proveedor para una mantención externa.");
    }
}
