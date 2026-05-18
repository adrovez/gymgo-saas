using GymGo.Domain.WorkoutLogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymGo.Infrastructure.Persistence.Configurations;

public sealed class WorkoutPlanExerciseConfiguration : IEntityTypeConfiguration<WorkoutPlanExercise>
{
    public void Configure(EntityTypeBuilder<WorkoutPlanExercise> builder)
    {
        builder.ToTable("WorkoutPlanExercises");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.WorkoutPlanDayId).IsRequired();

        builder.Property(e => e.ExerciseName).IsRequired().HasMaxLength(200);

        builder.Property(e => e.MuscleGroup)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.SortOrder).IsRequired();

        builder.Property(e => e.PlannedWeightKg).HasColumnType("decimal(6,2)");
        builder.Property(e => e.Notes).HasMaxLength(500);

        builder.HasIndex(e => new { e.WorkoutPlanDayId, e.SortOrder })
            .HasDatabaseName("IX_WorkoutPlanExercises_DayId");
    }
}
