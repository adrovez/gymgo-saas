using GymGo.Domain.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymGo.Infrastructure.Persistence.Configurations;

public sealed class SaleDiscountConfiguration : IEntityTypeConfiguration<SaleDiscount>
{
    public void Configure(EntityTypeBuilder<SaleDiscount> builder)
    {
        builder.ToTable("SaleDiscounts");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.TenantId).IsRequired();
        builder.Property(d => d.SaleId).IsRequired();

        builder.Property(d => d.DiscountType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(d => d.Description)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(d => d.Value)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(d => d.Amount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.HasIndex(d => d.SaleId)
            .HasDatabaseName("IX_SaleDiscounts_SaleId");
    }
}
