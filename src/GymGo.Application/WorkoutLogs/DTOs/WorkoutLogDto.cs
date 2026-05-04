using GymGo.Domain.WorkoutLogs;

namespace GymGo.Application.WorkoutLogs.DTOs;

/// <summary>
/// Resumen de una sesión de entrenamiento (sin ejercicios).
/// Usado en listados paginados.
/// </summary>
public sealed record WorkoutLogSummaryDto(
    Guid Id,
    Guid MemberId,
    DateOnly Date,
    string? Title,
    WorkoutLogStatus Status,
    string StatusName,
    int ExerciseCount,
    DateTime CreatedAtUtc
);

/// <summary>
/// Detalle completo de una sesión de entrenamiento con sus ejercicios.
/// </summary>
public sealed record WorkoutLogDto(
    Guid Id,
    Guid MemberId,
    DateOnly Date,
    string? Title,
    string? Notes,
    WorkoutLogStatus Status,
    string StatusName,
    IReadOnlyList<WorkoutLogExerciseDto> Exercises,
    DateTime CreatedAtUtc,
    DateTime? ModifiedAtUtc
);
