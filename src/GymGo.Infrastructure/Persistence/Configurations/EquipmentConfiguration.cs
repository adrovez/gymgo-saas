using GymGo.Domain.Equipments;
using GymGo.Domain.Maintenance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymGo.Infrastructure.Persistence.Configurations;

public sealed class EquipmentConfiguration : IEntityTypeConfiguration<Equipment>
{
    public void Configure(EntityTypeBuilder<Equipment> builder)
    {
        builder.ToTable("Equipment");

        builder.HasKey(e => e.Id);

        // ── Multi-tenant ───────────────────────────────────────────────
        builder.Property(e => e.TenantId).IsRequired();
        builder.HasIndex(e => e.TenantId)
            .HasDatabaseName("IX_Equipment_TenantId");

        builder.HasIndex(e => new { e.TenantId, e.IsActive })
            .HasDatabaseName("IX_Equipment_TenantId_IsActive");

        // ── Datos ──────────────────────────────────────────────────────
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Brand)
            .HasMaxLength(100);

        builder.Property(e => e.Model)
            .HasMaxLength(100);

        builder.Property(e => e.SerialNumber)
            .HasMaxLength(50);

        builder.Property(e => e.ImageUrl)
            .HasMaxLength(500);

        // ── Estado ────────────────────────────────────────────────────
        builder.Property(e => e.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // ── IAuditable ─────────────────────────────────────────────────
        builder.Property(e => e.CreatedAtUtc).IsRequired();
        builder.Property(e => e.CreatedBy).HasMaxLength(200);
        builder.Property(e => e.ModifiedBy).HasMaxLength(200);

        // ── ISoftDeletable ─────────────────────────────────────────────
        builder.Property(e => e.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(e => e.DeletedBy).HasMaxLength(200);

        // ── Navegación ─────────────────────────────────────────────────
        builder.HasMany(e => e.MaintenanceRecords)
            .WithOne(m => m.Equipment)
            .HasForeignKey(m => m.EquipmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(e => e.DomainEvents);
    }
}
