using GymGo.Domain.WorkoutLogs;

namespace GymGo.Application.WorkoutLogs.DTOs;

public sealed record WorkoutLogExerciseDto(
    Guid Id,
    Guid WorkoutLogId,
    Guid? WorkoutPlanExerciseId,
    string ExerciseName,
    MuscleGroup MuscleGroup,
    string MuscleGroupName,
    int SortOrder,
    bool IsExtra,
    int? ActualSets,
    int? ActualReps,
    decimal? ActualWeightKg,
    int? ActualDurationMinutes,
    int? ActualDistanceMeters,
    string? Notes
);
