using GymGo.Domain.WorkoutLogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymGo.Infrastructure.Persistence.Configurations;

public sealed class WorkoutPlanDayConfiguration : IEntityTypeConfiguration<WorkoutPlanDay>
{
    public void Configure(EntityTypeBuilder<WorkoutPlanDay> builder)
    {
        builder.ToTable("WorkoutPlanDays");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.WorkoutPlanId).IsRequired();

        builder.Property(d => d.DayOfWeek)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(d => d.Notes).HasMaxLength(500);

        builder.HasMany(d => d.Exercises)
            .WithOne()
            .HasForeignKey(e => e.WorkoutPlanDayId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(d => new { d.WorkoutPlanId, d.DayOfWeek })
            .IsUnique()
            .HasDatabaseName("UQ_WorkoutPlanDays_PlanDay");
    }
}
