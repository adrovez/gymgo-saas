namespace GymGo.Domain.Common;

/// <summary>
/// Marca una entidad como aislada por tenant. El ApplicationDbContext
/// aplica un HasQueryFilter para filtrar por TenantId actual y un
/// interceptor para asignarlo automáticamente al insertar.
/// </summary>
public interface ITenantScoped
{
    Guid TenantId { get; set; }
}
