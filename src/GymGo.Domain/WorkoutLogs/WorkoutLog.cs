using GymGo.Domain.Common;
using GymGo.Domain.Exceptions;

namespace GymGo.Domain.WorkoutLogs;

/// <summary>
/// Cabecera de una sesión de entrenamiento diario de un socio.
/// Agrupa todos los ejercicios realizados en un día dado.
///
/// Invariantes del dominio:
/// - Pertenece a un tenant y a un socio (ambos inmutables tras la creación).
/// - Solo puede haber un WorkoutLog en estado Draft por socio por día.
/// - Los ejercicios solo pueden agregarse/editarse mientras el log no esté Completed.
/// - Completar el log es irreversible desde el dominio.
/// </summary>
public sealed class WorkoutLog : AggregateRoot, IAuditable, ITenantScoped, ISoftDeletable
{
    // ── Tenant ────────────────────────────────────────────────────────────
    /// <inheritdoc/>
    public Guid TenantId { get; set; }

    // ── Relación con el socio ─────────────────────────────────────────────
    /// <summary>Socio que realizó el entrenamiento.</summary>
    public Guid MemberId { get; private set; }

    // ── Datos de la sesión ────────────────────────────────────────────────
    /// <summary>Fecha de la sesión de entrenamiento.</summary>
    public DateOnly Date { get; private set; }

    /// <summary>Título opcional de la rutina (ej: "Día A – Push", "Full Body Semana 3").</summary>
    public string? Title { get; private set; }

    /// <summary>Observaciones generales de la sesión.</summary>
    public string? Notes { get; private set; }

    /// <summary>Estado del log: Draft (en curso) o Completed (finalizado).</summary>
    public WorkoutLogStatus Status { get; private set; }

    // ── Ejercicios (navegación EF) ────────────────────────────────────────
    private readonly List<WorkoutLogExercise> _exercises = new();
    /// <summary>Ejercicios registrados en esta sesión.</summary>
    public IReadOnlyList<WorkoutLogExercise> Exercises => _exercises.AsReadOnly();

    // ── IAuditable ────────────────────────────────────────────────────────
    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedAtUtc { get; set; }
    public string? ModifiedBy { get; set; }

    // ── ISoftDeletable ────────────────────────────────────────────────────
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public string? DeletedBy { get; set; }

    // ── Constructor privado para EF Core ──────────────────────────────────
    private WorkoutLog() { }

    private WorkoutLog(
        Guid id,
        Guid tenantId,
        Guid memberId,
        DateOnly date,
        string? title,
        string? notes)
        : base(id)
    {
        TenantId = tenantId;
        MemberId = memberId;
        Date     = date;
        Title    = title;
        Notes    = notes;
        Status   = WorkoutLogStatus.Draft;
    }

    // ── Factory ───────────────────────────────────────────────────────────

    /// <summary>
    /// Crea una nueva sesión de entrenamiento para el socio indicado.
    /// </summary>
    /// <param name="tenantId">Gimnasio al que pertenece el registro.</param>
    /// <param name="memberId">Socio que entrena.</param>
    /// <param name="date">Fecha de la sesión. Si es null se usa la fecha actual UTC.</param>
    /// <param name="title">Título descriptivo de la rutina (opcional).</param>
    /// <param name="notes">Observaciones generales de la sesión (opcional).</param>
    public static WorkoutLog Create(
        Guid tenantId,
        Guid memberId,
        DateOnly? date = null,
        string? title = null,
        string? notes = null)
    {
        if (tenantId == Guid.Empty)
            throw new BusinessRuleViolationException(
                "WORKOUTLOG_TENANT_REQUIRED",
                "El registro de rutina debe pertenecer a un gimnasio (TenantId requerido).");

        if (memberId == Guid.Empty)
            throw new BusinessRuleViolationException(
                "WORKOUTLOG_MEMBER_REQUIRED",
                "El registro de rutina debe pertenecer a un socio (MemberId requerido).");

        if (title is not null && title.Length > 200)
            throw new BusinessRuleViolationException(
                "WORKOUTLOG_TITLE_TOO_LONG",
                "El título de la rutina no puede superar los 200 caracteres.");

        if (notes is not null && notes.Length > 1000)
            throw new BusinessRuleViolationException(
                "WORKOUTLOG_NOTES_TOO_LONG",
                "Las observaciones no pueden superar los 1000 caracteres.");

        var sessionDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);

        return new WorkoutLog(
            Guid.NewGuid(),
            tenantId,
            memberId,
            sessionDate,
            string.IsNullOrWhiteSpace(title) ? null : title.Trim(),
            string.IsNullOrWhiteSpace(notes) ? null : notes.Trim());
    }

    // ── Comportamiento ────────────────────────────────────────────────────

    /// <summary>
    /// Actualiza los datos de cabecera del log (solo mientras esté en Draft).
    /// </summary>
    public void Update(string? title, string? notes)
    {
        EnsureNotCompleted("editar");

        if (title is not null && title.Length > 200)
            throw new BusinessRuleViolationException(
                "WORKOUTLOG_TITLE_TOO_LONG",
                "El título de la rutina no puede superar los 200 caracteres.");

        if (notes is not null && notes.Length > 1000)
            throw new BusinessRuleViolationException(
                "WORKOUTLOG_NOTES_TOO_LONG",
                "Las observaciones no pueden superar los 1000 caracteres.");

        Title = string.IsNullOrWhiteSpace(title) ? null : title.Trim();
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }

    /// <summary>
    /// Marca la sesión como completada. Operación irreversible.
    /// </summary>
    public void Complete()
    {
        if (Status == WorkoutLogStatus.Completed)
            throw new BusinessRuleViolationException(
                "WORKOUTLOG_ALREADY_COMPLETED",
                "La sesión de entrenamiento ya fue marcada como completada.");

        if (_exercises.Count == 0)
            throw new BusinessRuleViolationException(
                "WORKOUTLOG_NO_EXERCISES",
                "No se puede completar una sesión sin ejercicios registrados.");

        Status = WorkoutLogStatus.Completed;
    }

    /// <summary>
    /// Agrega un ejercicio a la sesión (solo en Draft).
    /// </summary>
    public WorkoutLogExercise AddExercise(
        string exerciseName,
        MuscleGroup muscleGroup = MuscleGroup.NotSpecified,
        int? sets = null,
        int? reps = null,
        decimal? weightKg = null,
        int? durationSeconds = null,
        decimal? distanceMeters = null,
        string? notes = null)
    {
        EnsureNotCompleted("agregar ejercicios a");

        var sortOrder = _exercises.Count > 0
            ? _exercises.Max(e => e.SortOrder) + 1
            : 0;

        var exercise = WorkoutLogExercise.Create(
            workoutLogId:    Id,
            exerciseName:    exerciseName,
            muscleGroup:     muscleGroup,
            sortOrder:       sortOrder,
            sets:            sets,
            reps:            reps,
            weightKg:        weightKg,
            durationSeconds: durationSeconds,
            distanceMeters:  distanceMeters,
            notes:           notes);

        _exercises.Add(exercise);
        return exercise;
    }

    /// <summary>
    /// Actualiza los datos de un ejercicio existente en la sesión (solo en Draft).
    /// </summary>
    public void UpdateExercise(
        Guid exerciseId,
        string exerciseName,
        MuscleGroup muscleGroup,
        int sortOrder,
        int? sets,
        int? reps,
        decimal? weightKg,
        int? durationSeconds,
        decimal? distanceMeters,
        string? notes)
    {
        EnsureNotCompleted("editar ejercicios de");

        var exercise = _exercises.FirstOrDefault(e => e.Id == exerciseId)
            ?? throw new NotFoundException("WorkoutLogExercise", exerciseId);

        exercise.Update(exerciseName, muscleGroup, sortOrder, sets, reps,
            weightKg, durationSeconds, distanceMeters, notes);
    }

    /// <summary>
    /// Elimina un ejercicio de la sesión (solo en Draft).
    /// </summary>
    public void RemoveExercise(Guid exerciseId)
    {
        EnsureNotCompleted("eliminar ejercicios de");

        var exercise = _exercises.FirstOrDefault(e => e.Id == exerciseId)
            ?? throw new NotFoundException("WorkoutLogExercise", exerciseId);

        _exercises.Remove(exercise);
    }

    // ── Helpers privados ──────────────────────────────────────────────────

    private void EnsureNotCompleted(string action)
    {
        if (Status == WorkoutLogStatus.Completed)
            throw new BusinessRuleViolationException(
                "WORKOUTLOG_IS_COMPLETED",
                $"No se puede {action} una sesión de entrenamiento ya completada.");
    }
}
