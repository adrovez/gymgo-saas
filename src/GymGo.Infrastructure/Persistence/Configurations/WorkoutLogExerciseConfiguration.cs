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

        // ── Relación con la sesión ─────────────────────────────────────
        // La FK ya está configurada en WorkoutLogConfiguration (HasMany/WithOne).
        builder.Property(e => e.WorkoutLogId).IsRequired();

        // ── Ejercicio ──────────────────────────────────────────────────
        builder.Property(e => e.ExerciseName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.MuscleGroup)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(MuscleGroup.NotSpecified);

        builder.Property(e => e.SortOrder)
            .IsRequired()
            .HasDefaultValue(0);

        // ── Métricas ───────────────────────────────────────────────────
        builder.Property(e => e.WeightKg)
            .HasColumnType("decimal(6,2)");

        builder.Property(e => e.DistanceMeters)
            .HasColumnType("decimal(8,2)");

        // ── Notas ──────────────────────────────────────────────────────
        builder.Property(e => e.Notes)
            .HasMaxLength(500);

        // ── Índice para cargar ejercicios ordenados por sesión ─────────
        builder.HasIndex(e => new { e.WorkoutLogId, e.SortOrder })
            .HasDatabaseName("IX_WorkoutLogExercises_WorkoutLogId");
    }
}
