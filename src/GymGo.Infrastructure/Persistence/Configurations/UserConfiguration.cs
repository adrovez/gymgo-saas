using GymGo.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymGo.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.TenantId).IsRequired();

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(200);

        // Email único por tenant
        builder.HasIndex(u => new { u.TenantId, u.Email }).IsUnique();

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(u => u.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(u => u.Role)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(u => u.IsActive).HasDefaultValue(true);

        builder.Property(u => u.CreatedAtUtc).IsRequired();
        builder.Property(u => u.CreatedBy).HasMaxLength(200);
        builder.Property(u => u.ModifiedBy).HasMaxLength(200);
        builder.Property(u => u.DeletedBy).HasMaxLength(200);

        builder.HasIndex(u => u.TenantId);

        builder.Ignore(u => u.DomainEvents);
    }
}
