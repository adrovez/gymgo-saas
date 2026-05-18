using GymGo.Application.WorkoutPlans.DTOs;
using GymGo.Domain.WorkoutLogs;

namespace GymGo.Application.WorkoutLogs.DTOs;

public static class WorkoutLogMappings
{
    public static WorkoutLogExerciseDto ToDto(this WorkoutLogExercise e) =>
        new(
            Id:                   e.Id,
            WorkoutLogId:         e.WorkoutLogId,
            WorkoutPlanExerciseId: e.WorkoutPlanExerciseId,
            ExerciseName:         e.ExerciseName,
            MuscleGroup:          e.MuscleGroup,
            MuscleGroupName:      e.MuscleGroup.ToSpanish(),
            SortOrder:            e.SortOrder,
            IsExtra:              e.IsExtra,
            ActualSets:           e.ActualSets,
            ActualReps:           e.ActualReps,
            ActualWeightKg:       e.ActualWeightKg,
            ActualDurationMinutes: e.ActualDurationMinutes,
            ActualDistanceMeters: e.ActualDistanceMeters,
            Notes:                e.Notes
        );

    public static WorkoutLogDto ToDto(this WorkoutLog log, string dayOfWeekName) =>
        new(
            Id:               log.Id,
            MemberId:         log.MemberId,
            WorkoutPlanId:    log.WorkoutPlanId,
            WorkoutPlanDayId: log.WorkoutPlanDayId,
            DayOfWeekName:    dayOfWeekName,
            Date:             log.Date,
            Notes:            log.Notes,
            Status:           log.Status,
            StatusName:       log.Status.ToSpanish(),
            Exercises:        log.Exercises.OrderBy(e => e.SortOrder).Select(e => e.ToDto()).ToList(),
            CreatedAtUtc:     log.CreatedAtUtc,
            ModifiedAtUtc:    log.ModifiedAtUtc
        );

    public static WorkoutLogSummaryDto ToSummaryDto(this WorkoutLog log, string dayOfWeekName) =>
        new(
            Id:               log.Id,
            MemberId:         log.MemberId,
            WorkoutPlanId:    log.WorkoutPlanId,
            WorkoutPlanDayId: log.WorkoutPlanDayId,
            DayOfWeekName:    dayOfWeekName,
            Date:             log.Date,
            Status:           log.Status,
            StatusName:       log.Status.ToSpanish(),
            ExerciseCount:    log.Exercises.Count,
            CreatedAtUtc:     log.CreatedAtUtc
        );

    private static string ToSpanish(this WorkoutLogStatus status) => status switch
    {
        WorkoutLogStatus.Draft     => "En curso",
        WorkoutLogStatus.Completed => "Completada",
        _                          => status.ToString()
    };
}
