using GymGo.Domain.Common;
using GymGo.Domain.Exceptions;

namespace GymGo.Domain.GymEntries;

/// <summary>
/// Registro de ingreso de un socio al gimnasio (acceso general a las instalaciones).
///
/// Se crea únicamente cuando el acceso es válido: membresía activa,
/// día permitido y horario permitido. Los intentos de ingreso rechazados
/// lanzan excepciones de negocio y no se persisten.
///
/// Invariantes:
/// - MemberId y MembershipAssignmentId son inmutables.
/// - MemberFullName es un snapshot del nombre al momento del ingreso.
/// - EntryDate no puede ser futura.
/// </summary>
public sealed class GymEntry : AggregateRoot, IAuditable, ITenantScoped
{
    // ── Tenant ────────────────────────────────────────────────────────────
    public Guid TenantId { get; set; }

    // ── Relaciones ────────────────────────────────────────────────────────
    /// <summary>Socio que ingresa.</summary>
    public Guid MemberId { get; private set; }

    /// <summary>Asignación de membresía vigente usada para validar el acceso.</summary>
    public Guid MembershipAssignmentId { get; private set; }

    // ── Datos del ingreso ─────────────────────────────────────────────────
    /// <summary>Fecha del ingreso (solo parte de fecha).</summary>
    public DateOnly EntryDate { get; private set; }

    /// <summary>Timestamp exacto del ingreso (UTC).</summary>
    public DateTime EnteredAtUtc { get; private set; }

    /// <summary>Método utilizado para registrar el ingreso.</summary>
    public GymEntryMethod Method { get; private set; }

    /// <summary>
    /// Nombre completo del socio en el momento del ingreso (snapshot).
    /// Permite consultar el historial aunque el socio cambie de nombre.
    /// </summary>
    public string MemberFullName { get; private set; } = string.Empty;

    /// <summary>Observaciones opcionales de la recepcionista.</summary>
    public string? Notes { get; private set; }

    // ── IAuditable ────────────────────────────────────────────────────────
    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedAtUtc { get; set; }
    public string? ModifiedBy { get; set; }

    // ── Constructor privado para EF Core ──────────────────────────────────
    private GymEntry() { }

    private GymEntry(
        Guid id,
        Guid tenantId,
        Guid memberId,
        Guid membershipAssignmentId,
        DateOnly entryDate,
        DateTime enteredAtUtc,
        GymEntryMethod method,
        string memberFullName,
        string? notes)
        : base(id)
    {
        TenantId               = tenantId;
        MemberId               = memberId;
        MembershipAssignmentId = membershipAssignmentId;
        EntryDate              = entryDate;
        EnteredAtUtc           = enteredAtUtc;
        Method                 = method;
        MemberFullName         = memberFullName;
        Notes                  = notes;
    }

    // ── Factory ───────────────────────────────────────────────────────────

    /// <summary>
    /// Registra el ingreso de un socio al gimnasio.
    /// Las validaciones de membresía y restricciones de acceso se realizan
    /// en el handler antes de invocar este factory.
    /// </summary>
    public static GymEntry Create(
        Guid tenantId,
        Guid memberId,
        Guid membershipAssignmentId,
        DateOnly entryDate,
        DateTime enteredAtUtc,
        GymEntryMethod method,
        string memberFullName,
        string? notes = null)
    {
        if (tenantId == Guid.Empty)
            throw new BusinessRuleViolationException(
                "ENTRY_TENANT_REQUIRED", "El tenant es obligatorio.");

        if (memberId == Guid.Empty)
            throw new BusinessRuleViolationException(
                "ENTRY_MEMBER_REQUIRED", "El socio es obligatorio.");

        if (membershipAssignmentId == Guid.Empty)
            throw new BusinessRuleViolationException(
                "ENTRY_ASSIGNMENT_REQUIRED", "La asignación de membresía es obligatoria.");

        if (entryDate > DateOnly.FromDateTime(DateTime.UtcNow))
            throw new BusinessRuleViolationException(
                "ENTRY_DATE_FUTURE", "La fecha de ingreso no puede ser futura.");

        if (string.IsNullOrWhiteSpace(memberFullName))
            throw new BusinessRuleViolationException(
                "ENTRY_MEMBER_NAME_REQUIRED", "El nombre del socio es obligatorio.");

        if (notes is not null && notes.Length > 500)
            throw new BusinessRuleViolationException(
                "ENTRY_NOTES_TOO_LONG", "Las observaciones no pueden superar los 500 caracteres.");

        return new GymEntry(
            Guid.NewGuid(),
            tenantId,
            memberId,
            membershipAssignmentId,
            entryDate,
            enteredAtUtc,
            method,
            memberFullName.Trim(),
            string.IsNullOrWhiteSpace(notes) ? null : notes.Trim());
    }
}
