using GymGo.Domain.WorkoutLogs;

namespace GymGo.Application.WorkoutPlans.DTOs;

public static class WorkoutPlanMappings
{
    public static WorkoutPlanExerciseDto ToDto(this WorkoutPlanExercise e) =>
        new(
            Id:                    e.Id,
            WorkoutPlanDayId:      e.WorkoutPlanDayId,
            ExerciseName:          e.ExerciseName,
            MuscleGroup:           e.MuscleGroup,
            MuscleGroupName:       e.MuscleGroup.ToSpanish(),
            SortOrder:             e.SortOrder,
            PlannedSets:           e.PlannedSets,
            PlannedReps:           e.PlannedReps,
            PlannedWeightKg:       e.PlannedWeightKg,
            PlannedDurationMinutes: e.PlannedDurationMinutes,
            PlannedDistanceMeters: e.PlannedDistanceMeters,
            Notes:                 e.Notes
        );

    public static WorkoutPlanDayDto ToDto(this WorkoutPlanDay d) =>
        new(
            Id:             d.Id,
            WorkoutPlanId:  d.WorkoutPlanId,
            DayOfWeek:      d.DayOfWeek,
            DayOfWeekName:  d.DayOfWeek.ToSpanish(),
            Notes:          d.Notes,
            Exercises:      d.Exercises.OrderBy(e => e.SortOrder).Select(e => e.ToDto()).ToList()
        );

    public static WorkoutPlanDto ToDto(this WorkoutPlan p) =>
        new(
            Id:                      p.Id,
            MemberId:                p.MemberId,
            Objective:               p.Objective,
            StartDate:               p.StartDate,
            EndDate:                 p.EndDate,
            Notes:                   p.Notes,
            InitialWeightKg:         p.InitialWeightKg,
            InitialHeightCm:         p.InitialHeightCm,
            InitialBodyFatPercentage: p.InitialBodyFatPercentage,
            Status:                  p.Status,
            StatusName:              p.Status.ToSpanish(),
            Days:                    p.Days.OrderBy(d => d.DayOfWeek).Select(d => d.ToDto()).ToList(),
            CreatedAtUtc:            p.CreatedAtUtc,
            ModifiedAtUtc:           p.ModifiedAtUtc
        );

    public static WorkoutPlanSummaryDto ToSummaryDto(this WorkoutPlan p) =>
        new(
            Id:          p.Id,
            MemberId:    p.MemberId,
            Objective:   p.Objective,
            StartDate:   p.StartDate,
            EndDate:     p.EndDate,
            Status:      p.Status,
            StatusName:  p.Status.ToSpanish(),
            DayCount:    p.Days.Count,
            CreatedAtUtc: p.CreatedAtUtc
        );

    internal static string ToSpanish(this WorkoutPlanStatus status) => status switch
    {
        WorkoutPlanStatus.Active    => "Activa",
        WorkoutPlanStatus.Completed => "Completada",
        WorkoutPlanStatus.Cancelled => "Cancelada",
        _                           => status.ToString()
    };

    internal static string ToSpanish(this WorkoutDayOfWeek day) => day switch
    {
        WorkoutDayOfWeek.Monday    => "Lunes",
        WorkoutDayOfWeek.Tuesday   => "Martes",
        WorkoutDayOfWeek.Wednesday => "Miércoles",
        WorkoutDayOfWeek.Thursday  => "Jueves",
        WorkoutDayOfWeek.Friday    => "Viernes",
        WorkoutDayOfWeek.Saturday  => "Sábado",
        WorkoutDayOfWeek.Sunday    => "Domingo",
        _                          => day.ToString()
    };

    internal static string ToSpanish(this MuscleGroup group) => group switch
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
