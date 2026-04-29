using GymGo.Domain.GymClasses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymGo.Infrastructure.Persistence.Configurations;

public sealed class GymClassConfiguration : IEntityTypeConfiguration<GymClass>
{
    public void Configure(EntityTypeBuilder<GymClass> builder)
    {
        builder.ToTable("GymClasses");

        builder.HasKey(c => c.Id);

        // ── Multi-tenant ───────────────────────────────────────────────
        builder.Property(c => c.TenantId).IsRequired();
        builder.HasIndex(c => c.TenantId).HasDatabaseName("IX_GymClasses_TenantId");

        builder.HasIndex(c => new { c.TenantId, c.IsActive })
            .HasDatabaseName("IX_GymClasses_TenantId_IsActive");

        // ── Datos ──────────────────────────────────────────────────────
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        builder.Property(c => c.Category)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(ClassCategory.Other);

        builder.Property(c => c.Color)
            .HasMaxLength(7);

        builder.Property(c => c.DurationMinutes)
            .IsRequired()
            .HasDefaultValue(60);

        builder.Property(c => c.MaxCapacity)
            .IsRequired()
            .HasDefaultValue(20);

        // ── Estado ────────────────────────────────────────────────────
        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // ── IAuditable ─────────────────────────────────────────────────
        builder.Property(c => c.CreatedAtUtc).IsRequired();
        builder.Property(c => c.CreatedBy).HasMaxLength(200);
        builder.Property(c => c.ModifiedBy).HasMaxLength(200);

        // ── ISoftDeletable ─────────────────────────────────────────────
        builder.Property(c => c.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(c => c.DeletedBy).HasMaxLength(200);

        // ── Navegación ─────────────────────────────────────────────────
        builder.HasMany(c => c.Schedules)
            .WithOne(s => s.GymClass)
            .HasForeignKey(s => s.GymClassId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(c => c.DomainEvents);
    }
}
