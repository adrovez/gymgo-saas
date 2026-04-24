using GymGo.Domain.Members;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymGo.Infrastructure.Persistence.Configurations;

public sealed class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.ToTable("Members");

        builder.HasKey(m => m.Id);

        // ── Multi-tenant ───────────────────────────────────────────────
        builder.Property(m => m.TenantId).IsRequired();
        builder.HasIndex(m => m.TenantId).HasDatabaseName("IX_Members_TenantId");

        // ── Identificación ─────────────────────────────────────────────
        builder.Property(m => m.Rut)
            .IsRequired()
            .HasMaxLength(20);

        // RUT único por tenant entre registros no eliminados
        // (el filtrado WHERE IsDeleted = 0 se maneja a nivel SQL en el índice)
        builder.HasIndex(m => new { m.TenantId, m.Rut })
            .IsUnique()
            .HasDatabaseName("UX_Members_TenantId_Rut");

        builder.Property(m => m.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.BirthDate)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(m => m.Gender)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(Gender.NotSpecified);

        // ── Contacto ───────────────────────────────────────────────────
        builder.Property(m => m.Email)
            .HasMaxLength(200);

        builder.Property(m => m.Phone)
            .HasMaxLength(40);

        builder.Property(m => m.Address)
            .HasMaxLength(300);

        // ── Contacto de emergencia ─────────────────────────────────────
        builder.Property(m => m.EmergencyContactName)
            .HasMaxLength(200);

        builder.Property(m => m.EmergencyContactPhone)
            .HasMaxLength(40);

        // ── Estado y membresía ─────────────────────────────────────────
        builder.Property(m => m.Status)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(MemberStatus.Active);

        builder.Property(m => m.RegistrationDate)
            .IsRequired()
            .HasColumnType("date");

        // ── Observaciones ──────────────────────────────────────────────
        builder.Property(m => m.Notes)
            .HasMaxLength(1000);

        // ── IAuditable ─────────────────────────────────────────────────
        builder.Property(m => m.CreatedAtUtc).IsRequired();
        builder.Property(m => m.CreatedBy).HasMaxLength(200);
        builder.Property(m => m.ModifiedBy).HasMaxLength(200);

        // ── ISoftDeletable ─────────────────────────────────────────────
        builder.Property(m => m.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);
        builder.Property(m => m.DeletedBy).HasMaxLength(200);

        // ── Excluir eventos de dominio del mapeo ───────────────────────
        builder.Ignore(m => m.DomainEvents);
    }
}
