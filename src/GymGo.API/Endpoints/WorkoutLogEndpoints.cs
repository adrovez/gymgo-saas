using GymGo.Application.WorkoutLogs.Commands.AddExercise;
using GymGo.Application.WorkoutLogs.Commands.CompleteWorkoutLog;
using GymGo.Application.WorkoutLogs.Commands.CreateWorkoutLog;
using GymGo.Application.WorkoutLogs.Commands.DeleteWorkoutLog;
using GymGo.Application.WorkoutLogs.Commands.RemoveExercise;
using GymGo.Application.WorkoutLogs.Commands.UpdateExercise;
using GymGo.Application.WorkoutLogs.Commands.UpdateWorkoutLog;
using GymGo.Application.WorkoutLogs.Queries.GetWorkoutLogById;
using GymGo.Application.WorkoutLogs.Queries.GetWorkoutLogs;
using GymGo.Domain.WorkoutLogs;
using MediatR;

namespace GymGo.API.Endpoints;

public static class WorkoutLogEndpoints
{
    public static IEndpointRouteBuilder MapWorkoutLogEndpoints(this IEndpointRouteBuilder app)
    {
        // ── Crear sesión de entrenamiento ─────────────────────────────────────
        // POST /api/v1/workout-logs
        app.MapPost("/api/v1/workout-logs",
            async (CreateWorkoutLogRequest body, ISender sender, CancellationToken ct) =>
            {
                var command = new CreateWorkoutLogCommand(
                    MemberId: body.MemberId,
                    Date:     body.Date,
                    Title:    body.Title,
                    Notes:    body.Notes);

                var id = await sender.Send(command, ct);
                return Results.Created($"/api/v1/workout-logs/{id}", new { id });
            })
            .WithTags("WorkoutLogs")
            .WithSummary("Crear sesión de entrenamiento")
            .WithDescription(
                "Crea una nueva sesión de entrenamiento (WorkoutLog) para el socio indicado. " +
                "El estado inicial es Draft. No puede haber dos sesiones en Draft para el mismo socio en el mismo día.")
            .RequireAuthorization()
            .Produces(201)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(422)
            .ProducesProblem(401);

        // ── Historial de sesiones de un socio ─────────────────────────────────
        // GET /api/v1/workout-logs?memberId=...&from=2026-01-01&to=2026-04-30
        app.MapGet("/api/v1/workout-logs",
            async (Guid memberId, DateOnly? from, DateOnly? to, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new GetWorkoutLogsQuery(memberId, from, to), ct);
                return Results.Ok(result);
            })
            .WithTags("WorkoutLogs")
            .WithSummary("Historial de rutinas de un socio")
            .WithDescription(
                "Retorna el historial de sesiones de entrenamiento del socio indicado, " +
                "filtrable por rango de fechas (from/to). Ordenado por fecha descendente. " +
                "Incluye conteo de ejercicios por sesión.")
            .RequireAuthorization()
            .Produces(200)
            .ProducesProblem(400)
            .ProducesProblem(401);

        // ── Detalle de una sesión ─────────────────────────────────────────────
        // GET /api/v1/workout-logs/{id}
        app.MapGet("/api/v1/workout-logs/{id:guid}",
            async (Guid id, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new GetWorkoutLogByIdQuery(id), ct);
                return Results.Ok(result);
            })
            .WithTags("WorkoutLogs")
            .WithSummary("Obtener detalle de una sesión")
            .WithDescription(
                "Retorna el detalle completo de la sesión de entrenamiento indicada, " +
                "incluyendo todos sus ejercicios ordenados por SortOrder.")
            .RequireAuthorization()
            .Produces(200)
            .ProducesProblem(404)
            .ProducesProblem(401);

        // ── Editar cabecera de sesión ──────────────────────────────────────────
        // PUT /api/v1/workout-logs/{id}
        app.MapPut("/api/v1/workout-logs/{id:guid}",
            async (Guid id, UpdateWorkoutLogRequest body, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new UpdateWorkoutLogCommand(id, body.Title, body.Notes), ct);
                return Results.NoContent();
            })
            .WithTags("WorkoutLogs")
            .WithSummary("Editar título y notas de la sesión")
            .WithDescription(
                "Actualiza el título y las observaciones de la sesión. " +
                "Solo aplicable mientras la sesión esté en estado Draft.")
            .RequireAuthorization()
            .Produces(204)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(422)
            .ProducesProblem(401);

        // ── Completar sesión ──────────────────────────────────────────────────
        // PATCH /api/v1/workout-logs/{id}/complete
        app.MapPatch("/api/v1/workout-logs/{id:guid}/complete",
            async (Guid id, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new CompleteWorkoutLogCommand(id), ct);
                return Results.NoContent();
            })
            .WithTags("WorkoutLogs")
            .WithSummary("Completar sesión de entrenamiento")
            .WithDescription(
                "Marca la sesión como Completed. Operación irreversible. " +
                "La sesión debe tener al menos un ejercicio registrado.")
            .RequireAuthorization()
            .Produces(204)
            .ProducesProblem(404)
            .ProducesProblem(422)
            .ProducesProblem(401);

        // ── Eliminar sesión ───────────────────────────────────────────────────
        // DELETE /api/v1/workout-logs/{id}
        app.MapDelete("/api/v1/workout-logs/{id:guid}",
            async (Guid id, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new DeleteWorkoutLogCommand(id), ct);
                return Results.NoContent();
            })
            .WithTags("WorkoutLogs")
            .WithSummary("Eliminar sesión de entrenamiento")
            .WithDescription(
                "Realiza un soft delete de la sesión. " +
                "Los ejercicios asociados quedan eliminados en cascada.")
            .RequireAuthorization()
            .Produces(204)
            .ProducesProblem(404)
            .ProducesProblem(401);

        // ── Agregar ejercicio ─────────────────────────────────────────────────
        // POST /api/v1/workout-logs/{id}/exercises
        app.MapPost("/api/v1/workout-logs/{id:guid}/exercises",
            async (Guid id, AddExerciseRequest body, ISender sender, CancellationToken ct) =>
            {
                var command = new AddExerciseCommand(
                    WorkoutLogId:    id,
                    ExerciseName:    body.ExerciseName,
                    MuscleGroup:     body.MuscleGroup ?? MuscleGroup.NotSpecified,
                    Sets:            body.Sets,
                    Reps:            body.Reps,
                    WeightKg:        body.WeightKg,
                    DurationSeconds: body.DurationSeconds,
                    DistanceMeters:  body.DistanceMeters,
                    Notes:           body.Notes);

                var exerciseId = await sender.Send(command, ct);
                return Results.Created(
                    $"/api/v1/workout-logs/{id}/exercises/{exerciseId}",
                    new { id = exerciseId });
            })
            .WithTags("WorkoutLogs")
            .WithSummary("Agregar ejercicio a la sesión")
            .WithDescription(
                "Agrega un nuevo ejercicio al log de entrenamiento indicado. " +
                "Solo aplicable mientras la sesión esté en Draft. " +
                "El SortOrder se asigna automáticamente.")
            .RequireAuthorization()
            .Produces(201)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(422)
            .ProducesProblem(401);

        // ── Actualizar ejercicio ──────────────────────────────────────────────
        // PUT /api/v1/workout-logs/{id}/exercises/{exerciseId}
        app.MapPut("/api/v1/workout-logs/{id:guid}/exercises/{exerciseId:guid}",
            async (Guid id, Guid exerciseId, UpdateExerciseRequest body, ISender sender, CancellationToken ct) =>
            {
                var command = new UpdateExerciseCommand(
                    WorkoutLogId:    id,
                    ExerciseId:      exerciseId,
                    ExerciseName:    body.ExerciseName,
                    MuscleGroup:     body.MuscleGroup,
                    SortOrder:       body.SortOrder,
                    Sets:            body.Sets,
                    Reps:            body.Reps,
                    WeightKg:        body.WeightKg,
                    DurationSeconds: body.DurationSeconds,
                    DistanceMeters:  body.DistanceMeters,
                    Notes:           body.Notes);

                await sender.Send(command, ct);
                return Results.NoContent();
            })
            .WithTags("WorkoutLogs")
            .WithSummary("Actualizar ejercicio de la sesión")
            .WithDescription(
                "Actualiza todos los datos de un ejercicio dentro de la sesión indicada. " +
                "Solo aplicable mientras la sesión esté en Draft.")
            .RequireAuthorization()
            .Produces(204)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(422)
            .ProducesProblem(401);

        // ── Eliminar ejercicio ────────────────────────────────────────────────
        // DELETE /api/v1/workout-logs/{id}/exercises/{exerciseId}
        app.MapDelete("/api/v1/workout-logs/{id:guid}/exercises/{exerciseId:guid}",
            async (Guid id, Guid exerciseId, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new RemoveExerciseCommand(id, exerciseId), ct);
                return Results.NoContent();
            })
            .WithTags("WorkoutLogs")
            .WithSummary("Eliminar ejercicio de la sesión")
            .WithDescription(
                "Elimina un ejercicio del log de entrenamiento. " +
                "Solo aplicable mientras la sesión esté en Draft.")
            .RequireAuthorization()
            .Produces(204)
            .ProducesProblem(404)
            .ProducesProblem(422)
            .ProducesProblem(401);

        return app;
    }
}

// ── Request bodies ─────────────────────────────────────────────────────────────

/// <summary>Cuerpo del request para crear una sesión de entrenamiento.</summary>
public sealed record CreateWorkoutLogRequest(
    /// <summary>Id del socio que realizó el entrenamiento.</summary>
    Guid MemberId,
    /// <summary>Fecha de la sesión (null = hoy UTC).</summary>
    DateOnly? Date,
    /// <summary>Título descriptivo de la rutina (opcional).</summary>
    string? Title,
    /// <summary>Observaciones generales de la sesión (opcional).</summary>
    string? Notes
);

/// <summary>Cuerpo del request para actualizar la cabecera de una sesión.</summary>
public sealed record UpdateWorkoutLogRequest(
    string? Title,
    string? Notes
);

/// <summary>Cuerpo del request para agregar un ejercicio a la sesión.</summary>
public sealed record AddExerciseRequest(
    /// <summary>Nombre del ejercicio (ej: "Press de banca plano").</summary>
    string ExerciseName,
    /// <summary>Grupo muscular principal (0 = NotSpecified por defecto).</summary>
    MuscleGroup? MuscleGroup,
    /// <summary>Número de series realizadas.</summary>
    int? Sets,
    /// <summary>Repeticiones por serie.</summary>
    int? Reps,
    /// <summary>Peso utilizado en kg.</summary>
    decimal? WeightKg,
    /// <summary>Duración en segundos (ejercicios de tiempo).</summary>
    int? DurationSeconds,
    /// <summary>Distancia en metros (cardio).</summary>
    decimal? DistanceMeters,
    /// <summary>Notas específicas del ejercicio.</summary>
    string? Notes
);

/// <summary>Cuerpo del request para actualizar un ejercicio existente.</summary>
public sealed record UpdateExerciseRequest(
    string ExerciseName,
    MuscleGroup MuscleGroup,
    int SortOrder,
    int? Sets,
    int? Reps,
    decimal? WeightKg,
    int? DurationSeconds,
    decimal? DistanceMeters,
    string? Notes
);
