using GymGo.Domain.WorkoutLogs;

namespace GymGo.Application.WorkoutLogs.DTOs;

/// <summary>
/// Extensiones de mapeo de entidades de dominio a DTOs de WorkoutLogs.
/// </summary>
public static class WorkoutLogMappings
{
    public static WorkoutLogExerciseDto ToDto(this WorkoutLogExercise exercise) =>
        new(
            Id:              exercise.Id,
            WorkoutLogId:    exercise.WorkoutLogId,
            ExerciseName:    exercise.ExerciseName,
            MuscleGroup:     exercise.MuscleGroup,
            MuscleGroupName: exercise.MuscleGroup.ToSpanish(),
            SortOrder:       exercise.SortOrder,
            Sets:            exercise.Sets,
            Reps:            exercise.Reps,
            WeightKg:        exercise.WeightKg,
            DurationSeconds: exercise.DurationSeconds,
            DistanceMeters:  exercise.DistanceMeters,
            Notes:           exercise.Notes
        );

    public static WorkoutLogDto ToDto(this WorkoutLog log) =>
        new(
            Id:           log.Id,
            MemberId:     log.MemberId,
            Date:         log.Date,
            Title:        log.Title,
            Notes:        log.Notes,
            Status:       log.Status,
            StatusName:   log.Status.ToSpanish(),
            Exercises:    log.Exercises.Select(e => e.ToDto()).ToList(),
            CreatedAtUtc: log.CreatedAtUtc,
            ModifiedAtUtc: log.ModifiedAtUtc
        );

    public static WorkoutLogSummaryDto ToSummaryDto(this WorkoutLog log) =>
        new(
            Id:            log.Id,
            MemberId:      log.MemberId,
            Date:          log.Date,
            Title:         log.Title,
            Status:        log.Status,
            StatusName:    log.Status.ToSpanish(),
            ExerciseCount: log.Exercises.Count,
            CreatedAtUtc:  log.CreatedAtUtc
        );

    // ── Helpers de traducción ─────────────────────────────────────────────

    private static string ToSpanish(this WorkoutLogStatus status) => status switch
    {
        WorkoutLogStatus.Draft      => "En curso",
        WorkoutLogStatus.Completed  => "Completada",
        _                           => status.ToString()
    };

    private static string ToSpanish(this MuscleGroup group) => group switch
    {
        MuscleGroup.NotSpecified => "Sin especificar",
        MuscleGroup.Chest        => "Pecho",
        MuscleGroup.Back         => "Espalda",
        MuscleGroup.Shoulders    => "Hombros",
        MuscleGroup.Biceps       => "Bíceps",
        MuscleGroup.Triceps      => "Tríceps",
        MuscleGroup.Legs         => "Piernas",
        MuscleGroup.Core         => "Core / Abdomen",
        MuscleGroup.Glutes       => "Glúteos",
        MuscleGroup.Cardio       => "Cardio",
        MuscleGroup.FullBody     => "Cuerpo completo",
        _                        => group.ToString()
    };
}
