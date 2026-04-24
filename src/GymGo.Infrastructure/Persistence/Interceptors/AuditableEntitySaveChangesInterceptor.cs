using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace GymGo.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Llena CreatedAtUtc/CreatedBy y ModifiedAtUtc/ModifiedBy automáticamente.
/// También maneja soft-delete (transforma Deleted en Modified + IsDeleted=true).
/// </summary>
public sealed class AuditableEntitySaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUser _currentUser;
    private readonly IDateTimeProvider _clock;

    public AuditableEntitySaveChangesInterceptor(ICurrentUser currentUser, IDateTimeProvider clock)
    {
        _currentUser = currentUser;
        _clock = clock;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateEntities(DbContext? context)
    {
        if (context is null) return;

        var now = _clock.UtcNow;
        var who = _currentUser.Email ?? _currentUser.UserId?.ToString() ?? "system";

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity is IAuditable auditable)
            {
                if (entry.State == EntityState.Added)
                {
                    auditable.CreatedAtUtc = now;
                    auditable.CreatedBy = who;
                }
                else if (entry.State == EntityState.Modified || HasOwnedEntityChanged(entry))
                {
                    auditable.ModifiedAtUtc = now;
                    auditable.ModifiedBy = who;
                }
            }

            if (entry.Entity is ISoftDeletable soft && entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                soft.IsDeleted = true;
                soft.DeletedAtUtc = now;
                soft.DeletedBy = who;
            }
        }
    }

    private static bool HasOwnedEntityChanged(EntityEntry entry) =>
        entry.References.Any(r =>
            r.TargetEntry is not null &&
            r.TargetEntry.Metadata.IsOwned() &&
            (r.TargetEntry.State == EntityState.Added || r.TargetEntry.State == EntityState.Modified));
}
