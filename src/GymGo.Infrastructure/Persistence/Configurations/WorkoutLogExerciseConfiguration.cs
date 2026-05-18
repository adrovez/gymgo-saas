using GymGo.Domain.WorkoutLogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymGo.Infrastructure.Persistence.Configurations;

public sealed class WorkoutLogExerciseConfiguration : IEntityTypeConfiguration<WorkoutLogExercise>
{
    public void Configure(EntityTypeBuilder<WorkoutLogExercise> builder)
    {
        builder.ToTable("WorkoutLogExercises");

        builder.HasKey(e => e.Id);

        // FK al log — configurada también en WorkoutLogConfiguration (HasMany/WithOne)
        builder.Property(e => e.WorkoutLogId).IsRequired();

        // FK opcional al ejercicio planificado
        builder.Property(e => e.WorkoutPlanExerciseId);

        builder.HasOne<WorkoutPlanExercise>()
            .WithMany()
            .HasForeignKey(e => e.WorkoutPlanExerciseId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(e => e.ExerciseName).IsRequired().HasMaxLength(200);

        builder.Property(e => e.MuscleGroup)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.SortOrder).IsRequired();
        builder.Property(e => e.IsExtra).IsRequired().HasDefaultValue(false);

        builder.Property(e => e.ActualWeightKg).HasColumnType("decimal(6,2)");
        builder.Property(e => e.Notes).HasMaxLength(500);

        builder.HasIndex(e => new { e.WorkoutLogId, e.SortOrder })
            .HasDatabaseName("IX_WorkoutLogExercises_WorkoutLogId");
    }
}
