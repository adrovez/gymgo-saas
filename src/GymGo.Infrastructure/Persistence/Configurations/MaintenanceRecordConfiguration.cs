using GymGo.Domain.Maintenance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymGo.Infrastructure.Persistence.Configurations;

public sealed class MaintenanceRecordConfiguration : IEntityTypeConfiguration<MaintenanceRecord>
{
    public void Configure(EntityTypeBuilder<MaintenanceRecord> builder)
    {
        builder.ToTable("MaintenanceRecords");

        builder.HasKey(m => m.Id);

        // ── Multi-tenant ───────────────────────────────────────────────
        builder.Property(m => m.TenantId).IsRequired();
        builder.HasIndex(m => m.TenantId)
            .HasDatabaseName("IX_MR_TenantId");

        // ── Relación con máquina ───────────────────────────────────────
        builder.Property(m => m.EquipmentId).IsRequired();
        builder.HasIndex(m => m.EquipmentId)
            .HasDatabaseName("IX_MR_EquipmentId");

        builder.HasIndex(m => new { m.TenantId, m.Status, m.ScheduledDate })
            .HasDatabaseName("IX_MR_TenantId_Status_ScheduledDate");

        // ── Tipo y estado ──────────────────────────────────────────────
        builder.Property(m => m.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(m => m.Status)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(MaintenanceStatus.Pending);

        // ── Fechas ─────────────────────────────────────────────────────
        builder.Property(m => m.ScheduledDate).IsRequired();

        // ── Descripción ────────────────────────────────────────────────
        builder.Property(m => m.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(m => m.Notes)
            .HasMaxLength(1000);

        builder.Property(m => m.Cost)
            .HasColumnType("decimal(10,2)");

        // ── Responsable ────────────────────────────────────────────────
        builder.Property(m => m.ResponsibleType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(m => m.ExternalProviderName)
            .HasMaxLength(200);

        builder.Property(m => m.ExternalProviderContact)
            .HasMaxLength(200);

        // ── IAuditable ─────────────────────────────────────────────────
        builder.Property(m => m.CreatedAtUtc).IsRequired();
        builder.Property(m => m.CreatedBy).HasMaxLength(200);
        builder.Property(m => m.ModifiedBy).HasMaxLength(200);

        builder.Ignore(m => m.DomainEvents);
    }
}
