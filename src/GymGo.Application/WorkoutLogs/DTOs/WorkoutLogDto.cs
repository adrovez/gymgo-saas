using GymGo.Domain.WorkoutLogs;

namespace GymGo.Application.WorkoutLogs.DTOs;

public sealed record WorkoutLogSummaryDto(
    Guid Id,
    Guid MemberId,
    Guid WorkoutPlanId,
    Guid WorkoutPlanDayId,
    string DayOfWeekName,
    DateOnly Date,
    WorkoutLogStatus Status,
    string StatusName,
    int ExerciseCount,
    DateTime CreatedAtUtc
);

public sealed record WorkoutLogDto(
    Guid Id,
    Guid MemberId,
    Guid WorkoutPlanId,
    Guid WorkoutPlanDayId,
    string DayOfWeekName,
    DateOnly Date,
    string? Notes,
    WorkoutLogStatus Status,
    string StatusName,
    IReadOnlyList<WorkoutLogExerciseDto> Exercises,
    DateTime CreatedAtUtc,
    DateTime? ModifiedAtUtc
);
