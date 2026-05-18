using GymGo.Application.WorkoutPlans.Commands.AddPlanDay;
using GymGo.Application.WorkoutPlans.Commands.AddPlanExercise;
using GymGo.Application.WorkoutPlans.Commands.CreateWorkoutPlan;
using GymGo.Application.WorkoutPlans.Commands.DeleteWorkoutPlan;
using GymGo.Application.WorkoutPlans.Commands.RemovePlanDay;
using GymGo.Application.WorkoutPlans.Commands.RemovePlanExercise;
using GymGo.Application.WorkoutPlans.Commands.UpdatePlanExercise;
using GymGo.Application.WorkoutPlans.Commands.UpdateWorkoutPlan;
using GymGo.Application.WorkoutPlans.Queries.GetWorkoutPlanById;
using GymGo.Application.WorkoutPlans.Queries.GetWorkoutPlans;
using GymGo.Domain.WorkoutLogs;
using MediatR;

namespace GymGo.API.Endpoints;

public static class WorkoutPlanEndpoints
{
    public static IEndpointRouteBuilder MapWorkoutPlanEndpoints(this IEndpointRouteBuilder app)
    {
        // POST /api/v1/workout-plans
        app.MapPost("/api/v1/workout-plans",
            async (CreateWorkoutPlanRequest body, ISender sender, CancellationToken ct) =>
            {
                var id = await sender.Send(new CreateWorkoutPlanCommand(
                    body.MemberId,
                    body.Objective,
                    body.StartDate,
                    body.EndDate,
                    body.Notes,
                    body.InitialWeightKg,
                    body.InitialHeightCm,
                    body.InitialBodyFatPercentage), ct);

                return Results.Created($"/api/v1/workout-plans/{id}", new { id });
            })
            .WithTags("WorkoutPlans")
            .WithSummary("Crear rutina de entrenamiento")
            .RequireAuthorization()
            .Produces(201).ProducesProblem(400).ProducesProblem(422).ProducesProblem(401);

        // GET /api/v1/workout-plans?memberId=...&status=0
        app.MapGet("/api/v1/workout-plans",
            async (Guid memberId, WorkoutPlanStatus? status, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new GetWorkoutPlansQuery(memberId, status), ct);
                return Results.Ok(result);
            })
            .WithTags("WorkoutPlans")
            .WithSummary("Listar rutinas de un socio")
            .RequireAuthorization()
            .Produces(200).ProducesProblem(401);

        // GET /api/v1/workout-plans/{id}
        app.MapGet("/api/v1/workout-plans/{id:guid}",
            async (Guid id, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new GetWorkoutPlanByIdQuery(id), ct);
                return Results.Ok(result);
            })
            .WithTags("WorkoutPlans")
            .WithSummary("Obtener detalle de una rutina")
            .RequireAuthorization()
            .Produces(200).ProducesProblem(404).ProducesProblem(401);

        // PUT /api/v1/workout-plans/{id}
        app.MapPut("/api/v1/workout-plans/{id:guid}",
            async (Guid id, UpdateWorkoutPlanRequest body, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new UpdateWorkoutPlanCommand(
                    id,
                    body.Objective,
                    body.StartDate,
                    body.EndDate,
                    body.Notes,
                    body.InitialWeightKg,
                    body.InitialHeightCm,
                    body.InitialBodyFatPercentage), ct);

                return Results.NoContent();
            })
            .WithTags("WorkoutPlans")
            .WithSummary("Editar rutina")
            .RequireAuthorization()
            .Produces(204).ProducesProblem(400).ProducesProblem(404).ProducesProblem(422).ProducesProblem(401);

        // DELETE /api/v1/workout-plans/{id}
        app.MapDelete("/api/v1/workout-plans/{id:guid}",
            async (Guid id, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new DeleteWorkoutPlanCommand(id), ct);
                return Results.NoContent();
            })
            .WithTags("WorkoutPlans")
            .WithSummary("Eliminar rutina")
            .RequireAuthorization()
            .Produces(204).ProducesProblem(404).ProducesProblem(401);

        // ── Días ──────────────────────────────────────────────────────────────

        // POST /api/v1/workout-plans/{id}/days
        app.MapPost("/api/v1/workout-plans/{id:guid}/days",
            async (Guid id, AddPlanDayRequest body, ISender sender, CancellationToken ct) =>
            {
                var dayId = await sender.Send(new AddPlanDayCommand(id, body.DayOfWeek, body.Notes), ct);
                return Results.Created($"/api/v1/workout-plans/{id}", new { id = dayId });
            })
            .WithTags("WorkoutPlans")
            .WithSummary("Agregar día a la rutina")
            .RequireAuthorization()
            .Produces(201).ProducesProblem(400).ProducesProblem(404).ProducesProblem(422).ProducesProblem(401);

        // DELETE /api/v1/workout-plans/{id}/days/{dayId}
        app.MapDelete("/api/v1/workout-plans/{id:guid}/days/{dayId:guid}",
            async (Guid id, Guid dayId, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new RemovePlanDayCommand(id, dayId), ct);
                return Results.NoContent();
            })
            .WithTags("WorkoutPlans")
            .WithSummary("Eliminar día de la rutina")
            .RequireAuthorization()
            .Produces(204).ProducesProblem(404).ProducesProblem(422).ProducesProblem(401);

        // ── Ejercicios del plan ───────────────────────────────────────────────

        // POST /api/v1/workout-plans/days/{dayId}/exercises
        app.MapPost("/api/v1/workout-plans/days/{dayId:guid}/exercises",
            async (Guid dayId, AddPlanExerciseRequest body, ISender sender, CancellationToken ct) =>
            {
                var exerciseId = await sender.Send(new AddPlanExerciseCommand(
                    dayId,
                    body.ExerciseName,
                    body.MuscleGroup ?? MuscleGroup.NotSpecified,
                    body.PlannedSets,
                    body.PlannedReps,
                    body.PlannedWeightKg,
                    body.PlannedDurationMinutes,
                    body.PlannedDistanceMeters,
                    body.Notes), ct);

                return Results.Created(string.Empty, new { id = exerciseId });
            })
            .WithTags("WorkoutPlans")
            .WithSummary("Agregar ejercicio al día de rutina")
            .RequireAuthorization()
            .Produces(201).ProducesProblem(400).ProducesProblem(404).ProducesProblem(422).ProducesProblem(401);

        // PUT /api/v1/workout-plans/days/{dayId}/exercises/{exerciseId}
        app.MapPut("/api/v1/workout-plans/days/{dayId:guid}/exercises/{exerciseId:guid}",
            async (Guid dayId, Guid exerciseId, UpdatePlanExerciseRequest body, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new UpdatePlanExerciseCommand(
                    dayId,
                    exerciseId,
                    body.ExerciseName,
                    body.MuscleGroup,
                    body.SortOrder,
                    body.PlannedSets,
                    body.PlannedReps,
                    body.PlannedWeightKg,
                    body.PlannedDurationMinutes,
                    body.PlannedDistanceMeters,
                    body.Notes), ct);

                return Results.NoContent();
            })
            .WithTags("WorkoutPlans")
            .WithSummary("Actualizar ejercicio del plan")
            .RequireAuthorization()
            .Produces(204).ProducesProblem(400).ProducesProblem(404).ProducesProblem(422).ProducesProblem(401);

        // DELETE /api/v1/workout-plans/days/{dayId}/exercises/{exerciseId}
        app.MapDelete("/api/v1/workout-plans/days/{dayId:guid}/exercises/{exerciseId:guid}",
            async (Guid dayId, Guid exerciseId, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new RemovePlanExerciseCommand(dayId, exerciseId), ct);
                return Results.NoContent();
            })
            .WithTags("WorkoutPlans")
            .WithSummary("Eliminar ejercicio del plan")
            .RequireAuthorization()
            .Produces(204).ProducesProblem(404).ProducesProblem(422).ProducesProblem(401);

        return app;
    }
}

// ── Request bodies ─────────────────────────────────────────────────────────────

public sealed record CreateWorkoutPlanRequest(
    Guid MemberId,
    string Objective,
    DateOnly StartDate,
    DateOnly EndDate,
    string? Notes,
    decimal? InitialWeightKg,
    decimal? InitialHeightCm,
    decimal? InitialBodyFatPercentage
);

public sealed record UpdateWorkoutPlanRequest(
    string Objective,
    DateOnly StartDate,
    DateOnly EndDate,
    string? Notes,
    decimal? InitialWeightKg,
    decimal? InitialHeightCm,
    decimal? InitialBodyFatPercentage
);

public sealed record AddPlanDayRequest(
    WorkoutDayOfWeek DayOfWeek,
    string? Notes
);

public sealed record AddPlanExerciseRequest(
    string ExerciseName,
    MuscleGroup? MuscleGroup,
    int? PlannedSets,
    int? PlannedReps,
    decimal? PlannedWeightKg,
    int? PlannedDurationMinutes,
    int? PlannedDistanceMeters,
    string? Notes
);

public sealed record UpdatePlanExerciseRequest(
    string ExerciseName,
    MuscleGroup MuscleGroup,
    int SortOrder,
    int? PlannedSets,
    int? PlannedReps,
    decimal? PlannedWeightKg,
    int? PlannedDurationMinutes,
    int? PlannedDistanceMeters,
    string? Notes
);
