using GymGo.Domain.WorkoutLogs;

namespace GymGo.Application.WorkoutPlans.DTOs;

public sealed record WorkoutPlanExerciseDto(
    Guid Id,
    Guid WorkoutPlanDayId,
    string ExerciseName,
    MuscleGroup MuscleGroup,
    string MuscleGroupName,
    int SortOrder,
    int? PlannedSets,
    int? PlannedReps,
    decimal? PlannedWeightKg,
    int? PlannedDurationMinutes,
    int? PlannedDistanceMeters,
    string? Notes
);

public sealed record WorkoutPlanDayDto(
    Guid Id,
    Guid WorkoutPlanId,
    WorkoutDayOfWeek DayOfWeek,
    string DayOfWeekName,
    string? Notes,
    IReadOnlyList<WorkoutPlanExerciseDto> Exercises
);

public sealed record WorkoutPlanDto(
    Guid Id,
    Guid MemberId,
    string Objective,
    DateOnly StartDate,
    DateOnly EndDate,
    string? Notes,
    decimal? InitialWeightKg,
    decimal? InitialHeightCm,
    decimal? InitialBodyFatPercentage,
    WorkoutPlanStatus Status,
    string StatusName,
    IReadOnlyList<WorkoutPlanDayDto> Days,
    DateTime CreatedAtUtc,
    DateTime? ModifiedAtUtc
);

public sealed record WorkoutPlanSummaryDto(
    Guid Id,
    Guid MemberId,
    string Objective,
    DateOnly StartDate,
    DateOnly EndDate,
    WorkoutPlanStatus Status,
    string StatusName,
    int DayCount,
    DateTime CreatedAtUtc
);
