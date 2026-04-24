namespace GymGo.Domain.Common;

/// <summary>
/// Marca una entidad como borrable lógicamente.
/// EF Core aplica HasQueryFilter para excluir IsDeleted = true.
/// </summary>
public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAtUtc { get; set; }
    string? DeletedBy { get; set; }
}
