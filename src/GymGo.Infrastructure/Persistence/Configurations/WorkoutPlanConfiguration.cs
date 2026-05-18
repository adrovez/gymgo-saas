using GymGo.Domain.WorkoutLogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymGo.Infrastructure.Persistence.Configurations;

public sealed class WorkoutPlanConfiguration : IEntityTypeConfiguration<WorkoutPlan>
{
    public void Configure(EntityTypeBuilder<WorkoutPlan> builder)
    {
        builder.ToTable("WorkoutPlans");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.TenantId).IsRequired();
        builder.Property(p => p.MemberId).IsRequired();

        builder.HasOne<Domain.Members.Member>()
            .WithMany()
            .HasForeignKey(p => p.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(p => p.Objective).IsRequired().HasMaxLength(500);
        builder.Property(p => p.StartDate).IsRequired().HasColumnType("date");
        builder.Property(p => p.EndDate).IsRequired().HasColumnType("date");
        builder.Property(p => p.Notes).HasMaxLength(1000);

        builder.Property(p => p.InitialWeightKg).HasColumnType("decimal(5,2)");
        builder.Property(p => p.InitialHeightCm).HasColumnType("decimal(5,2)");
        builder.Property(p => p.InitialBodyFatPercentage).HasColumnType("decimal(5,2)");

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(WorkoutPlanStatus.Active);

        builder.HasMany(p => p.Days)
            .WithOne()
            .HasForeignKey(d => d.WorkoutPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => new { p.TenantId, p.MemberId, p.Status })
            .HasDatabaseName("IX_WorkoutPlans_Member_Status");

        builder.Property(p => p.CreatedAtUtc).IsRequired();
        builder.Property(p => p.CreatedBy).HasMaxLength(200);
        builder.Property(p => p.ModifiedBy).HasMaxLength(200);

        builder.Property(p => p.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(p => p.DeletedBy).HasMaxLength(200);

        builder.Ignore(p => p.DomainEvents);
    }
}
