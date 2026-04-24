namespace GymGo.Domain.Common;

/// <summary>
/// Marca una entidad como auditable. Un interceptor de EF Core
/// completa CreatedAt/Utc y ModifiedAt/Utc automáticamente.
/// </summary>
public interface IAuditable
{
    DateTime CreatedAtUtc { get; set; }
    string? CreatedBy { get; set; }
    DateTime? ModifiedAtUtc { get; set; }
    string? ModifiedBy { get; set; }
}
