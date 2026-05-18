using GymGo.Domain.Common;
using GymGo.Domain.Exceptions;

namespace GymGo.Domain.WorkoutLogs;

public sealed class WorkoutPlanDay : Entity
{
    public Guid WorkoutPlanId { get; private set; }
    public WorkoutDayOfWeek DayOfWeek { get; private set; }
    public string? Notes { get; private set; }

    private readonly List<WorkoutPlanExercise> _exercises = new();
    public IReadOnlyList<WorkoutPlanExercise> Exercises => _exercises.AsReadOnly();

    private WorkoutPlanDay() { }

    private WorkoutPlanDay(Guid id, Guid workoutPlanId, WorkoutDayOfWeek dayOfWeek, string? notes)
        : base(id)
    {
        WorkoutPlanId = workoutPlanId;
        DayOfWeek     = dayOfWeek;
        Notes         = notes;
    }

    public static WorkoutPlanDay Create(Guid workoutPlanId, WorkoutDayOfWeek dayOfWeek, string? notes = null)
    {
        if (workoutPlanId == Guid.Empty)
            throw new BusinessRuleViolationException(
                "PLANDAY_PLAN_REQUIRED",
                "El día debe pertenecer a una rutina.");

        if (notes is not null && notes.Length > 500)
            throw new BusinessRuleViolationException(
                "PLANDAY_NOTES_TOO_LONG",
                "Las notas del día no pueden superar los 500 caracteres.");

        return new WorkoutPlanDay(
            Guid.NewGuid(),
            workoutPlanId,
            dayOfWeek,
            string.IsNullOrWhiteSpace(notes) ? null : notes.Trim());
    }

    public void UpdateNotes(string? notes)
    {
        if (notes is not null && notes.Length > 500)
            throw new BusinessRuleViolationException(
                "PLANDAY_NOTES_TOO_LONG",
                "Las notas del día no pueden superar los 500 caracteres.");

        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }

    public WorkoutPlanExercise AddExercise(
        string exerciseName,
        MuscleGroup muscleGroup = MuscleGroup.NotSpecified,
        int? plannedSets = null,
        int? plannedReps = null,
        decimal? plannedWeightKg = null,
        int? plannedDurationMinutes = null,
        int? plannedDistanceMeters = null,
        string? notes = null)
    {
        var sortOrder = _exercises.Count > 0
            ? _exercises.Max(e => e.SortOrder) + 1
            : 0;

        var exercise = WorkoutPlanExercise.Create(
            workoutPlanDayId:       Id,
            exerciseName:           exerciseName,
            muscleGroup:            muscleGroup,
            sortOrder:              sortOrder,
            plannedSets:            plannedSets,
            plannedReps:            plannedReps,
            plannedWeightKg:        plannedWeightKg,
            plannedDurationMinutes: plannedDurationMinutes,
            plannedDistanceMeters:  plannedDistanceMeters,
            notes:                  notes);

        _exercises.Add(exercise);
        return exercise;
    }

    public void UpdateExercise(
        Guid exerciseId,
        string exerciseName,
        MuscleGroup muscleGroup,
        int sortOrder,
        int? plannedSets,
        int? plannedReps,
        decimal? plannedWeightKg,
        int? plannedDurationMinutes,
        int? plannedDistanceMeters,
        string? notes)
    {
        var exercise = _exercises.FirstOrDefault(e => e.Id == exerciseId)
            ?? throw new NotFoundException("WorkoutPlanExercise", exerciseId);

        exercise.Update(exerciseName, muscleGroup, sortOrder, plannedSets, plannedReps,
            plannedWeightKg, plannedDurationMinutes, plannedDistanceMeters, notes);
    }

    public void RemoveExercise(Guid exerciseId)
    {
        var exercise = _exercises.FirstOrDefault(e => e.Id == exerciseId)
            ?? throw new NotFoundException("WorkoutPlanExercise", exerciseId);

        _exercises.Remove(exercise);
    }
}
