using GymGo.Domain.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymGo.Infrastructure.Persistence.Configurations;

public sealed class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> builder)
    {
        builder.ToTable("SaleItems");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.TenantId).IsRequired();
        builder.Property(i => i.SaleId).IsRequired();

        builder.Property(i => i.ItemType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(i => i.Description)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.MembershipPlanId);

        builder.Property(i => i.UnitPrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(i => i.Quantity)
            .IsRequired()
            .HasDefaultValue(1);

        // Subtotal es calculado (propiedad computed en el dominio, no persiste)
        builder.Ignore(i => i.Subtotal);

        builder.HasIndex(i => i.SaleId)
            .HasDatabaseName("IX_SaleItems_SaleId");
    }
}
