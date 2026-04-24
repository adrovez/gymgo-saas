namespace GymGo.Application.Common.Interfaces;

/// <summary>
/// Resuelve el tenant actual del request. Lo lee de un claim o
/// de un header (X-Tenant-Id). Es la base del multi-tenancy.
/// </summary>
public interface ICurrentTenant
{
    Guid? TenantId { get; }
    bool HasTenant { get; }
}
