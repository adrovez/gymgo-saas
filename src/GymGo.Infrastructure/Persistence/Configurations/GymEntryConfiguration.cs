using GymGo.Domain.GymEntries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymGo.Infrastructure.Persistence.Configurations;

public sealed class GymEntryConfiguration : IEntityTypeConfiguration<GymEntry>
{
    public void Configure(EntityTypeBuilder<GymEntry> builder)
    {
        builder.ToTable("GymEntries");

        builder.HasKey(e => e.Id);

        // ── Multi-tenant ───────────────────────────────────────────────
        builder.Property(e => e.TenantId).IsRequired();

        // ── Relaciones ─────────────────────────────────────────────────
        builder.Property(e => e.MemberId).IsRequired();
        builder.Property(e => e.MembershipAssignmentId).IsRequired();

        builder
            .HasOne<Domain.Members.Member>()
            .WithMany()
            .HasForeignKey(e => e.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne<Domain.MembershipAssignments.MembershipAssignment>()
            .WithMany()
            .HasForeignKey(e => e.MembershipAssignmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── Datos del ingreso ──────────────────────────────────────────
        builder.Property(e => e.EntryDate)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(e => e.EnteredAtUtc)
            .IsRequired();

        builder.Property(e => e.Method)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(GymEntryMethod.Manual);

        builder.Property(e => e.MemberFullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Notes)
            .HasMaxLength(500);

        builder.Property(e => e.ExitedAtUtc)
            .IsRequired(false);

        // ── Índices de consulta ────────────────────────────────────────
        // Historial de ingresos de un socio
        builder.HasIndex(e => new { e.TenantId, e.MemberId, e.EntryDate })
            .HasDatabaseName("IX_GymEntries_Member_Date");

        // Listado de ingresos del día para el tenant (vista de recepción)
        builder.HasIndex(e => new { e.TenantId, e.EntryDate })
            .HasDatabaseName("IX_GymEntries_Tenant_Date");

        // ── IAuditable ─────────────────────────────────────────────────
        builder.Property(e => e.CreatedAtUtc).IsRequired();
        builder.Property(e => e.CreatedBy).HasMaxLength(200);
        builder.Property(e => e.ModifiedBy).HasMaxLength(200);

        // ── Excluir eventos de dominio del mapeo ───────────────────────
        builder.Ignore(e => e.DomainEvents);
    }
}
