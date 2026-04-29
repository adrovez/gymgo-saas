using GymGo.Domain.ClassReservations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymGo.Infrastructure.Persistence.Configurations;

public sealed class ClassReservationConfiguration : IEntityTypeConfiguration<ClassReservation>
{
    public void Configure(EntityTypeBuilder<ClassReservation> builder)
    {
        builder.ToTable("ClassReservations");

        builder.HasKey(r => r.Id);

        // ── Multi-tenant ───────────────────────────────────────────────────
        builder.Property(r => r.TenantId).IsRequired();

        // ── Relaciones ─────────────────────────────────────────────────────
        builder.Property(r => r.MemberId).IsRequired();
        builder.Property(r => r.ClassScheduleId).IsRequired();

        builder
            .HasOne<Domain.Members.Member>()
            .WithMany()
            .HasForeignKey(r => r.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne<Domain.GymClasses.ClassSchedule>()
            .WithMany()
            .HasForeignKey(r => r.ClassScheduleId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── Datos de la sesión ─────────────────────────────────────────────
        builder.Property(r => r.SessionDate)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(r => r.ReservedAtUtc)
            .IsRequired();

        builder.Property(r => r.MemberFullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.Notes)
            .HasMaxLength(500);

        // ── Estado ─────────────────────────────────────────────────────────
        builder.Property(r => r.Status)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(ReservationStatus.Active);

        builder.Property(r => r.CancelledBy)
            .HasMaxLength(200);

        builder.Property(r => r.CancelReason)
            .HasMaxLength(500);

        // ── Índices de consulta ────────────────────────────────────────────
        // Lista de reservas de una sesión (ClassScheduleId + SessionDate)
        builder.HasIndex(r => new { r.ClassScheduleId, r.SessionDate })
            .HasDatabaseName("IX_ClassReservations_Schedule_Date");

        // Historial de reservas de un socio
        builder.HasIndex(r => new { r.TenantId, r.MemberId, r.SessionDate })
            .HasDatabaseName("IX_ClassReservations_Member");

        // Unicidad: un socio no puede tener dos reservas activas para la misma sesión.
        // Esto se valida en el handler; el índice es una red de seguridad adicional.
        // No es un índice único convencional porque permite varios estados distintos
        // (canceladas previas + una activa), así que la unicidad se garantiza por código.

        // ── IAuditable ─────────────────────────────────────────────────────
        builder.Property(r => r.CreatedAtUtc).IsRequired();
        builder.Property(r => r.CreatedBy).HasMaxLength(200);
        builder.Property(r => r.ModifiedBy).HasMaxLength(200);

        // ── Excluir eventos de dominio ─────────────────────────────────────
        builder.Ignore(r => r.DomainEvents);
    }
}
