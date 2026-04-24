using GymGo.Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymGo.Infrastructure.Persistence.Configurations;

public sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(t => t.Slug)
            .IsRequired()
            .HasMaxLength(60);

        builder.HasIndex(t => t.Slug).IsUnique();

        builder.Property(t => t.ContactEmail).HasMaxLength(200);
        builder.Property(t => t.ContactPhone).HasMaxLength(40);

        builder.Property(t => t.IsActive).HasDefaultValue(true);

        builder.Property(t => t.CreatedAtUtc).IsRequired();
        builder.Property(t => t.CreatedBy).HasMaxLength(200);
        builder.Property(t => t.ModifiedBy).HasMaxLength(200);

        builder.Ignore(t => t.DomainEvents);
    }
}
