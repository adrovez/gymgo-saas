using GymGo.Domain.Cash;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymGo.Infrastructure.Persistence.Configurations;

public sealed class CashTransactionConfiguration : IEntityTypeConfiguration<CashTransaction>
{
    public void Configure(EntityTypeBuilder<CashTransaction> builder)
    {
        builder.ToTable("CashTransactions");

        builder.HasKey(t => t.Id);

        // ── Tenant ─────────────────────────────────────────────────────────
        builder.Property(t => t.TenantId).IsRequired();

        // ── Clasificación ──────────────────────────────────────────────────
        builder.Property(t => t.Date)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(t => t.Type)
            .IsRequired()
            .HasConversion<byte>();

        builder.Property(t => t.Amount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(t => t.PaymentMethod)
            .IsRequired()
            .HasConversion<byte>();

        builder.Property(t => t.Concept)
            .IsRequired()
            .HasConversion<byte>();

        // ── Descripción ────────────────────────────────────────────────────
        builder.Property(t => t.Description).HasMaxLength(500);

        // ── Vínculos opcionales ────────────────────────────────────────────
        builder.Property(t => t.MemberId);
        builder.Property(t => t.MembershipAssignmentId);

        // ── Auditoría ──────────────────────────────────────────────────────
        builder.Property(t => t.ProcessedByUserId).IsRequired();
        builder.Property(t => t.CreatedAtUtc).IsRequired();

        // ── Anulación ──────────────────────────────────────────────────────
        builder.Property(t => t.IsVoided).IsRequired().HasDefaultValue(false);
        builder.Property(t => t.VoidedAtUtc);
        builder.Property(t => t.VoidReason).HasMaxLength(500);

        // ── Índices ────────────────────────────────────────────────────────
        builder.HasIndex(t => new { t.TenantId, t.Date })
            .HasDatabaseName("IX_CashTransactions_TenantId_Date");

        builder.HasIndex(t => t.MemberId)
            .HasDatabaseName("IX_CashTransactions_MemberId")
            .HasFilter("[MemberId] IS NOT NULL");

        builder.Ignore(t => t.DomainEvents);
    }
}
