using GymGo.Domain.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymGo.Infrastructure.Persistence.Configurations;

public sealed class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("Sales");

        builder.HasKey(s => s.Id);

        // ── Tenant ─────────────────────────────────────────────────────────
        builder.Property(s => s.TenantId).IsRequired();

        // ── Socio ──────────────────────────────────────────────────────────
        builder.Property(s => s.MemberId).IsRequired();

        // ── Clasificación ──────────────────────────────────────────────────
        builder.Property(s => s.Date)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(s => s.Status)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(SaleStatus.Draft);

        builder.Property(s => s.PaymentMethod)
            .IsRequired()
            .HasConversion<byte>();

        // ── Montos ────────────────────────────────────────────────────────
        builder.Property(s => s.SubtotalAmount)
            .IsRequired()
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0m);

        builder.Property(s => s.DiscountAmount)
            .IsRequired()
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0m);

        builder.Property(s => s.TotalAmount)
            .IsRequired()
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0m);

        // ── Vínculos opcionales ────────────────────────────────────────────
        builder.Property(s => s.MembershipAssignmentId);
        builder.Property(s => s.CashTransactionId);

        // ── Observaciones ──────────────────────────────────────────────────
        builder.Property(s => s.Notes).HasMaxLength(500);

        // ── Auditoría ──────────────────────────────────────────────────────
        builder.Property(s => s.ProcessedByUserId).IsRequired();

        // ── Anulación ──────────────────────────────────────────────────────
        builder.Property(s => s.VoidedAtUtc);
        builder.Property(s => s.VoidReason).HasMaxLength(500);

        // ── IAuditable ─────────────────────────────────────────────────────
        builder.Property(s => s.CreatedAtUtc).IsRequired();
        builder.Property(s => s.CreatedBy).HasMaxLength(200);
        builder.Property(s => s.ModifiedBy).HasMaxLength(200);

        // ── ISoftDeletable ─────────────────────────────────────────────────
        builder.Property(s => s.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(s => s.DeletedBy).HasMaxLength(200);

        // ── Navegación ─────────────────────────────────────────────────────
        builder.HasMany(s => s.Items)
            .WithOne()
            .HasForeignKey(i => i.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Discounts)
            .WithOne()
            .HasForeignKey(d => d.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Índices ────────────────────────────────────────────────────────
        builder.HasIndex(s => new { s.TenantId, s.Date })
            .HasDatabaseName("IX_Sales_TenantId_Date");

        builder.HasIndex(s => s.MemberId)
            .HasDatabaseName("IX_Sales_MemberId");

        builder.HasIndex(s => new { s.TenantId, s.Status, s.Date })
            .HasDatabaseName("IX_Sales_TenantId_Status");

        builder.Ignore(s => s.DomainEvents);
    }
}
