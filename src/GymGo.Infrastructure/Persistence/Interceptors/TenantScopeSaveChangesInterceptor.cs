using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace GymGo.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Asegura que toda entidad ITenantScoped insertada lleve el TenantId
/// del contexto actual. Si se intenta insertar con TenantId distinto,
/// lanza una excepción (defensa en profundidad además de HasQueryFilter).
/// </summary>
public sealed class TenantScopeSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentTenant _currentTenant;

    public TenantScopeSaveChangesInterceptor(ICurrentTenant currentTenant)
    {
        _currentTenant = currentTenant;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        AssignTenant(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        AssignTenant(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void AssignTenant(DbContext? context)
    {
        if (context is null || !_currentTenant.HasTenant) return;

        var tenantId = _currentTenant.TenantId!.Value;

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity is not ITenantScoped scoped) continue;

            if (entry.State == EntityState.Added)
            {
                if (scoped.TenantId == Guid.Empty)
                    scoped.TenantId = tenantId;
                else if (scoped.TenantId != tenantId)
                    throw new InvalidOperationException(
                        $"Cross-tenant insert detectado. Esperado {tenantId}, recibido {scoped.TenantId}.");
            }
            else if (entry.State == EntityState.Modified)
            {
                // Bloquea reasignación de tenant en updates.
                if (scoped.TenantId != tenantId)
                    throw new InvalidOperationException(
                        $"Cross-tenant update detectado. Esperado {tenantId}, recibido {scoped.TenantId}.");
            }
        }
    }
}
