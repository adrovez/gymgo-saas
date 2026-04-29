using GymGo.Domain.GymClasses;

namespace GymGo.Application.GymClasses.DTOs;

public static class GymClassMappings
{
    public static GymClassDto ToDto(this GymClass c) => new(
        Id:             c.Id,
        TenantId:       c.TenantId,
        Name:           c.Name,
        Description:    c.Description,
        Category:       c.Category,
        CategoryLabel:  c.Category.ToLabel(),
        Color:          c.Color,
        DurationMinutes: c.DurationMinutes,
        MaxCapacity:    c.MaxCapacity,
        IsActive:       c.IsActive,
        CreatedAtUtc:   c.CreatedAtUtc,
        CreatedBy:      c.CreatedBy,
        ModifiedAtUtc:  c.ModifiedAtUtc,
        ModifiedBy:     c.ModifiedBy,
        Schedules:      c.Schedules
                         .Where(s => !s.IsDeleted)
                         .OrderBy(s => s.DayOfWeek)
                         .ThenBy(s => s.StartTime)
                         .Select(s => s.ToScheduleDto(c.Name, c.Color))
                         .ToList()
    );

    public static GymClassSummaryDto ToSummaryDto(this GymClass c, int scheduleCount = 0) => new(
        Id:             c.Id,
        Name:           c.Name,
        Description:    c.Description,
        Category:       c.Category,
        CategoryLabel:  c.Category.ToLabel(),
        Color:          c.Color,
        DurationMinutes: c.DurationMinutes,
        MaxCapacity:    c.MaxCapacity,
        IsActive:       c.IsActive,
        ScheduleCount:  scheduleCount
    );

    public static ClassScheduleDto ToScheduleDto(
        this ClassSchedule s,
        string gymClassName,
        string? gymClassColor) => new(
        Id:             s.Id,
        GymClassId:     s.GymClassId,
        GymClassName:   gymClassName,
        GymClassColor:  gymClassColor,
        DayOfWeek:      (int)s.DayOfWeek,
        DayLabel:       s.DayOfWeek.ToLabel(),
        StartTime:      s.StartTime.ToString("HH:mm"),
        EndTime:        s.EndTime.ToString("HH:mm"),
        InstructorName: s.InstructorName,
        Room:           s.Room,
        MaxCapacity:    s.MaxCapacity,
        IsActive:       s.IsActive
    );

    // ── Helpers de etiquetas ──────────────────────────────────────────────

    public static string ToLabel(this ClassCategory cat) => cat switch
    {
        ClassCategory.Cardio      => "Cardio",
        ClassCategory.Strength    => "Fuerza",
        ClassCategory.Flexibility => "Flexibilidad",
        ClassCategory.Martial     => "Artes marciales",
        ClassCategory.Dance       => "Baile",
        ClassCategory.Aquatic     => "Acuático",
        ClassCategory.Mind        => "Mente y cuerpo",
        _                         => "Otro",
    };

    public static string ToLabel(this DayOfWeek day) => day switch
    {
        DayOfWeek.Sunday    => "Domingo",
        DayOfWeek.Monday    => "Lunes",
        DayOfWeek.Tuesday   => "Martes",
        DayOfWeek.Wednesday => "Miércoles",
        DayOfWeek.Thursday  => "Jueves",
        DayOfWeek.Friday    => "Viernes",
        DayOfWeek.Saturday  => "Sábado",
        _                   => day.ToString(),
    };
}
