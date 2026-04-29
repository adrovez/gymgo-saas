using GymGo.Domain.GymClasses;

namespace GymGo.Application.GymClasses.DTOs;

/// <summary>Detalle completo de una clase con sus horarios.</summary>
public sealed record GymClassDto(
    Guid Id,
    Guid TenantId,
    string Name,
    string? Description,
    ClassCategory Category,
    string CategoryLabel,
    string? Color,
    int DurationMinutes,
    int MaxCapacity,
    bool IsActive,
    DateTime CreatedAtUtc,
    string? CreatedBy,
    DateTime? ModifiedAtUtc,
    string? ModifiedBy,
    IReadOnlyList<ClassScheduleDto> Schedules
);

/// <summary>Resumen de una clase para listados.</summary>
public sealed record GymClassSummaryDto(
    Guid Id,
    string Name,
    string? Description,
    ClassCategory Category,
    string CategoryLabel,
    string? Color,
    int DurationMinutes,
    int MaxCapacity,
    bool IsActive,
    int ScheduleCount
);

/// <summary>Horario semanal de una clase.</summary>
public sealed record ClassScheduleDto(
    Guid Id,
    Guid GymClassId,
    string GymClassName,
    string? GymClassColor,
    int DayOfWeek,
    string DayLabel,
    string StartTime,
    string EndTime,
    string? InstructorName,
    string? Room,
    int? MaxCapacity,
    bool IsActive
);
