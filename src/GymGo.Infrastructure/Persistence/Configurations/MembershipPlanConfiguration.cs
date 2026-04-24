using GymGo.Domain.MembershipPlans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymGo.Infrastructure.Persistence.Configurations;

public sealed class MembershipPlanConfiguration : IEntityTypeConfiguration<MembershipPlan>
{
    public void Configure(EntityTypeBuilder<MembershipPlan> builder)
    {
        builder.ToTable("MembershipPlans");

        builder.HasKey(p => p.Id);

        // ── Multi-tenant ───────────────────────────────────────────────
        builder.Property(p => p.TenantId).IsRequired();
        builder.HasIndex(p => p.TenantId).HasDatabaseName("IX_MembershipPlans_TenantId");

        // ── Identidad comercial ────────────────────────────────────────
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        // ── Periodicidad ───────────────────────────────────────────────
        builder.Property(p => p.Periodicity)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(p => p.DurationDays)
            .IsRequired();

        // ── Asistencia ─────────────────────────────────────────────────
        builder.Property(p => p.DaysPerWeek).IsRequired();
        builder.Property(p => p.FixedDays).IsRequired().HasDefaultValue(false);
        builder.Property(p => p.Monday).IsRequired().HasDefaultValue(false);
        builder.Property(p => p.Tuesday).IsRequired().HasDefaultValue(false);
        builder.Property(p => p.Wednesday).IsRequired().HasDefaultValue(false);
        builder.Property(p => p.Thursday).IsRequired().HasDefaultValue(false);
        builder.Property(p => p.Friday).IsRequired().HasDefaultValue(false);
        builder.Property(p => p.Saturday).IsRequired().HasDefaultValue(false);
        builder.Property(p => p.Sunday).IsRequired().HasDefaultValue(false);

        // ── Horario ────────────────────────────────────────────────────
        builder.Property(p => p.FreeSchedule).IsRequired().HasDefaultValue(true);

        // TimeOnly se mapea a TIME en SQL Server
        builder.Property(p => p.TimeFrom)
            .HasColumnType("time");

        builder.Property(p => p.TimeTo)
            .HasColumnType("time");

        // ── Comercial ──────────────────────────────────────────────────
        builder.Property(p => p.Amount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.AllowsFreezing)
            .IsRequired()
            .HasDefaultValue(false);

        // ── Ciclo de vida ──────────────────────────────────────────────
        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // ── IAuditable ─────────────────────────────────────────────────
        builder.Property(p => p.CreatedAtUtc).IsRequired();
        builder.Property(p => p.CreatedBy).HasMaxLength(200);
        builder.Property(p => p.ModifiedBy).HasMaxLength(200);

        // ── ISoftDeletable ─────────────────────────────────────────────
        builder.Property(p => p.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(p => p.DeletedBy).HasMaxLength(200);

        builder.Ignore(p => p.DomainEvents);
    }
}
