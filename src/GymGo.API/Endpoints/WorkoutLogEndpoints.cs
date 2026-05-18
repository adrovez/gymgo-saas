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
        // POST /api/v1/workout-logs
        app.MapPost("/api/v1/workout-logs",
            async (CreateWorkoutLogRequest body, ISender sender, CancellationToken ct) =>
            {
                var id = await sender.Send(new CreateWorkoutLogCommand(
                    body.MemberId,
                    body.WorkoutPlanId,
                    body.WorkoutPlanDayId,
                    body.Date,
                    body.Notes), ct);

                return Results.Created($"/api/v1/workout-logs/{id}", new { id });
            })
            .WithTags("WorkoutLogs")
            .WithSummary("Registrar sesión de entrenamiento")
            .WithDescription("Crea un registro de avances para el día de rutina indicado. El estado inicial es Draft.")
            .RequireAuthorization()
            .Produces(201).ProducesProblem(400).ProducesProblem(404).ProducesProblem(422).ProducesProblem(401);

        // GET /api/v1/workout-logs?memberId=...&workoutPlanId=...&from=...&to=...
        app.MapGet("/api/v1/workout-logs",
            async (Guid memberId, Guid? workoutPlanId, DateOnly? from, DateOnly? to, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new GetWorkoutLogsQuery(memberId, workoutPlanId, from, to), ct);
                return Results.Ok(result);
            })
            .WithTags("WorkoutLogs")
            .WithSummary("Historial de sesiones de un socio")
            .RequireAuthorization()
            .Produces(200).ProducesProblem(401);

        // GET /api/v1/workout-logs/{id}
        app.MapGet("/api/v1/workout-logs/{id:guid}",
            async (Guid id, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new GetWorkoutLogByIdQuery(id), ct);
                return Results.Ok(result);
            })
            .WithTags("WorkoutLogs")
            .WithSummary("Obtener detalle de una sesión")
            .RequireAuthorization()
            .Produces(200).ProducesProblem(404).ProducesProblem(401);

        // PUT /api/v1/workout-logs/{id}
        app.MapPut("/api/v1/workout-logs/{id:guid}",
            async (Guid id, UpdateWorkoutLogRequest body, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new UpdateWorkoutLogCommand(id, body.Notes), ct);
                return Results.NoContent();
            })
            .WithTags("WorkoutLogs")
            .WithSummary("Editar notas de la sesión")
            .RequireAuthorization()
            .Produces(204).ProducesProblem(400).ProducesProblem(404).ProducesProblem(422).ProducesProblem(401);

        // PATCH /api/v1/workout-logs/{id}/complete
        app.MapPatch("/api/v1/workout-logs/{id:guid}/complete",
            async (Guid id, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new CompleteWorkoutLogCommand(id), ct);
                return Results.NoContent();
            })
            .WithTags("WorkoutLogs")
            .WithSummary("Completar sesión de entrenamiento")
            .RequireAuthorization()
            .Produces(204).ProducesProblem(404).ProducesProblem(422).ProducesProblem(401);

        // DELETE /api/v1/workout-logs/{id}
        app.MapDelete("/api/v1/workout-logs/{id:guid}",
            async (Guid id, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new DeleteWorkoutLogCommand(id), ct);
                return Results.NoContent();
            })
            .WithTags("WorkoutLogs")
            .WithSummary("Eliminar sesión de entrenamiento")
            .RequireAuthorization()
            .Produces(204).ProducesProblem(404).ProducesProblem(401);

        // POST /api/v1/workout-logs/{id}/exercises
        app.MapPost("/api/v1/workout-logs/{id:guid}/exercises",
            async (Guid id, AddLogExerciseRequest body, ISender sender, CancellationToken ct) =>
            {
                var exerciseId = await sender.Send(new AddExerciseCommand(
                    WorkoutLogId:          id,
                    ExerciseName:          body.ExerciseName,
                    MuscleGroup:           body.MuscleGroup ?? MuscleGroup.NotSpecified,
                    WorkoutPlanExerciseId: body.WorkoutPlanExerciseId,
                    IsExtra:               body.IsExtra,
                    ActualSets:            body.ActualSets,
                    ActualReps:            body.ActualReps,
                    ActualWeightKg:        body.ActualWeightKg,
                    ActualDurationMinutes: body.ActualDurationMinutes,
                    ActualDistanceMeters:  body.ActualDistanceMeters,
                    Notes:                 body.Notes), ct);

                return Results.Created(
                    $"/api/v1/workout-logs/{id}/exercises/{exerciseId}",
                    new { id = exerciseId });
            })
            .WithTags("WorkoutLogs")
            .WithSummary("Registrar ejercicio en la sesión")
            .RequireAuthorization()
            .Produces(201).ProducesProblem(400).ProducesProblem(404).ProducesProblem(422).ProducesProblem(401);

        // PUT /api/v1/workout-logs/{id}/exercises/{exerciseId}
        app.MapPut("/api/v1/workout-logs/{id:guid}/exercises/{exerciseId:guid}",
            async (Guid id, Guid exerciseId, UpdateLogExerciseRequest body, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new UpdateExerciseCommand(
                    WorkoutLogId:          id,
                    ExerciseId:            exerciseId,
                    ExerciseName:          body.ExerciseName,
                    MuscleGroup:           body.MuscleGroup,
                    SortOrder:             body.SortOrder,
                    ActualSets:            body.ActualSets,
                    ActualReps:            body.ActualReps,
                    ActualWeightKg:        body.ActualWeightKg,
                    ActualDurationMinutes: body.ActualDurationMinutes,
                    ActualDistanceMeters:  body.ActualDistanceMeters,
                    Notes:                 body.Notes), ct);

                return Results.NoContent();
            })
            .WithTags("WorkoutLogs")
            .WithSummary("Actualizar ejercicio de la sesión")
            .RequireAuthorization()
            .Produces(204).ProducesProblem(400).ProducesProblem(404).ProducesProblem(422).ProducesProblem(401);

        // DELETE /api/v1/workout-logs/{id}/exercises/{exerciseId}
        app.MapDelete("/api/v1/workout-logs/{id:guid}/exercises/{exerciseId:guid}",
            async (Guid id, Guid exerciseId, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new RemoveExerciseCommand(id, exerciseId), ct);
                return Results.NoContent();
            })
            .WithTags("WorkoutLogs")
            .WithSummary("Eliminar ejercicio de la sesión")
            .RequireAuthorization()
            .Produces(204).ProducesProblem(404).ProducesProblem(422).ProducesProblem(401);

        return app;
    }
}

// ── Request bodies ─────────────────────────────────────────────────────────────

public sealed record CreateWorkoutLogRequest(
    Guid MemberId,
    Guid WorkoutPlanId,
    Guid WorkoutPlanDayId,
    DateOnly? Date,
    string? Notes
);

public sealed record UpdateWorkoutLogRequest(string? Notes);

public sealed record AddLogExerciseRequest(
    string ExerciseName,
    MuscleGroup? MuscleGroup,
    Guid? WorkoutPlanExerciseId,
    bool IsExtra,
    int? ActualSets,
    int? ActualReps,
    decimal? ActualWeightKg,
    int? ActualDurationMinutes,
    int? ActualDistanceMeters,
    string? Notes
);

public sealed record UpdateLogExerciseRequest(
    string ExerciseName,
    MuscleGroup MuscleGroup,
    int SortOrder,
    int? ActualSets,
    int? ActualReps,
    decimal? ActualWeightKg,
    int? ActualDurationMinutes,
    int? ActualDistanceMeters,
    string? Notes
);
