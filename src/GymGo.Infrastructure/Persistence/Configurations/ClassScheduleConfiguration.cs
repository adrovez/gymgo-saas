using GymGo.Domain.GymClasses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymGo.Infrastructure.Persistence.Configurations;

public sealed class ClassScheduleConfiguration : IEntityTypeConfiguration<ClassSchedule>
{
    public void Configure(EntityTypeBuilder<ClassSchedule> builder)
    {
        builder.ToTable("ClassSchedules");

        builder.HasKey(s => s.Id);

        // ── Multi-tenant ───────────────────────────────────────────────
        builder.Property(s => s.TenantId).IsRequired();
        builder.HasIndex(s => s.TenantId).HasDatabaseName("IX_ClassSchedules_TenantId");

        // ── Relaciones ─────────────────────────────────────────────────
        builder.Property(s => s.GymClassId).IsRequired();
        builder.HasIndex(s => s.GymClassId).HasDatabaseName("IX_ClassSchedules_GymClassId");

        // Índice compuesto para calendario semanal
        builder.HasIndex(s => new { s.TenantId, s.DayOfWeek, s.StartTime })
            .HasDatabaseName("IX_ClassSchedules_TenantId_DayOfWeek");

        // ── Horario ────────────────────────────────────────────────────
        builder.Property(s => s.DayOfWeek)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(s => s.StartTime)
            .IsRequired()
            .HasColumnType("time(0)");

        builder.Property(s => s.EndTime)
            .IsRequired()
            .HasColumnType("time(0)");

        // ── Datos operacionales ────────────────────────────────────────
        builder.Property(s => s.InstructorName).HasMaxLength(100);
        builder.Property(s => s.Room).HasMaxLength(100);

        // ── Estado ────────────────────────────────────────────────────
        builder.Property(s => s.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // ── IAuditable ─────────────────────────────────────────────────
        builder.Property(s => s.CreatedAtUtc).IsRequired();
        builder.Property(s => s.CreatedBy).HasMaxLength(200);
        builder.Property(s => s.ModifiedBy).HasMaxLength(200);

        // ── ISoftDeletable ─────────────────────────────────────────────
        builder.Property(s => s.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(s => s.DeletedBy).HasMaxLength(200);
    }
}
