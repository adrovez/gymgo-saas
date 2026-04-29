using GymGo.Domain.ClassAttendances;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymGo.Infrastructure.Persistence.Configurations;

public sealed class ClassAttendanceConfiguration : IEntityTypeConfiguration<ClassAttendance>
{
    public void Configure(EntityTypeBuilder<ClassAttendance> builder)
    {
        builder.ToTable("ClassAttendances");

        builder.HasKey(a => a.Id);

        // ── Multi-tenant ───────────────────────────────────────────────
        builder.Property(a => a.TenantId).IsRequired();

        // ── Relaciones ─────────────────────────────────────────────────
        builder.Property(a => a.MemberId).IsRequired();
        builder.Property(a => a.ClassScheduleId).IsRequired();

        builder
            .HasOne<Domain.Members.Member>()
            .WithMany()
            .HasForeignKey(a => a.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne<Domain.GymClasses.ClassSchedule>()
            .WithMany()
            .HasForeignKey(a => a.ClassScheduleId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── Datos de la sesión ─────────────────────────────────────────
        builder.Property(a => a.SessionDate)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(a => a.CheckedInAtUtc)
            .IsRequired();

        builder.Property(a => a.CheckInMethod)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(CheckInMethod.Manual);

        builder.Property(a => a.MemberFullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.Notes)
            .HasMaxLength(500);

        // ── Unicidad: un socio, un horario, una fecha ──────────────────
        builder.HasIndex(a => new { a.MemberId, a.ClassScheduleId, a.SessionDate })
            .IsUnique()
            .HasDatabaseName("UQ_ClassAttendances_Member_Schedule_Date");

        // ── Índices de consulta ────────────────────────────────────────
        builder.HasIndex(a => new { a.TenantId, a.ClassScheduleId, a.SessionDate })
            .HasDatabaseName("IX_ClassAttendances_Schedule_Date");

        builder.HasIndex(a => new { a.TenantId, a.MemberId, a.SessionDate })
            .HasDatabaseName("IX_ClassAttendances_Member");

        // ── IAuditable ─────────────────────────────────────────────────
        builder.Property(a => a.CreatedAtUtc).IsRequired();
        builder.Property(a => a.CreatedBy).HasMaxLength(200);
        builder.Property(a => a.ModifiedBy).HasMaxLength(200);

        // ── Excluir eventos de dominio del mapeo ───────────────────────
        builder.Ignore(a => a.DomainEvents);
    }
}
