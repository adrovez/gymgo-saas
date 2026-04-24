using GymGo.Domain.MembershipAssignments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymGo.Infrastructure.Persistence.Configurations;

public sealed class MembershipAssignmentConfiguration : IEntityTypeConfiguration<MembershipAssignment>
{
    public void Configure(EntityTypeBuilder<MembershipAssignment> builder)
    {
        builder.ToTable("MembershipAssignments");

        builder.HasKey(a => a.Id);

        // ── Multi-tenant ───────────────────────────────────────────────
        builder.Property(a => a.TenantId).IsRequired();
        builder.HasIndex(a => a.TenantId).HasDatabaseName("IX_MembershipAssignments_TenantId");

        // ── Relaciones ─────────────────────────────────────────────────
        builder.Property(a => a.MemberId).IsRequired();
        builder.Property(a => a.MembershipPlanId).IsRequired();

        // FK a Members
        builder.HasIndex(a => a.MemberId).HasDatabaseName("IX_MembershipAssignments_MemberId");

        // Índice compuesto para buscar asignación activa de un socio rápidamente
        builder.HasIndex(a => new { a.MemberId, a.Status })
            .HasDatabaseName("IX_MembershipAssignments_MemberId_Status");

        // ── Período ────────────────────────────────────────────────────
        builder.Property(a => a.StartDate)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(a => a.EndDate)
            .IsRequired()
            .HasColumnType("date");

        // ── Snapshot comercial ─────────────────────────────────────────
        builder.Property(a => a.AmountSnapshot)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        // ── Estado ────────────────────────────────────────────────────
        builder.Property(a => a.Status)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(AssignmentStatus.Active);

        builder.Property(a => a.PaymentStatus)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(Domain.MembershipAssignments.PaymentStatus.Pending);

        // ── Congelamiento ──────────────────────────────────────────────
        builder.Property(a => a.FrozenSince)
            .HasColumnType("date");

        builder.Property(a => a.FrozenDaysAccumulated)
            .IsRequired()
            .HasDefaultValue(0);

        // ── Observaciones ──────────────────────────────────────────────
        builder.Property(a => a.Notes).HasMaxLength(500);

        // ── IAuditable ─────────────────────────────────────────────────
        builder.Property(a => a.CreatedAtUtc).IsRequired();
        builder.Property(a => a.CreatedBy).HasMaxLength(200);
        builder.Property(a => a.ModifiedBy).HasMaxLength(200);

        // ── ISoftDeletable ─────────────────────────────────────────────
        builder.Property(a => a.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(a => a.DeletedBy).HasMaxLength(200);

        builder.Ignore(a => a.DomainEvents);
    }
}
