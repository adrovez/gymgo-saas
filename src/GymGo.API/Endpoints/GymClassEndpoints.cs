using GymGo.Application.GymClasses.Commands.CreateClassSchedule;
using GymGo.Application.GymClasses.Commands.CreateGymClass;
using GymGo.Application.GymClasses.Commands.DeactivateGymClass;
using GymGo.Application.GymClasses.Commands.DeleteClassSchedule;
using GymGo.Application.GymClasses.Commands.ReactivateGymClass;
using GymGo.Application.GymClasses.Commands.UpdateClassSchedule;
using GymGo.Application.GymClasses.Commands.UpdateGymClass;
using GymGo.Application.GymClasses.Queries.GetGymClassById;
using GymGo.Application.GymClasses.Queries.GetGymClasses;
using GymGo.Application.GymClasses.Queries.GetWeeklySchedule;
using GymGo.Domain.GymClasses;
using MediatR;

namespace GymGo.API.Endpoints;

public static class GymClassEndpoints
{
    public static IEndpointRouteBuilder MapGymClassEndpoints(this IEndpointRouteBuilder app)
    {
        // ── Listado de clases ────────────────────────────────────────────────
        app.MapGet("/api/v1/classes",
            async (bool? isActive, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new GetGymClassesQuery(isActive), ct);
                return Results.Ok(result);
            })
            .WithTags("GymClasses")
            .WithSummary("Listado de clases del gimnasio")
            .RequireAuthorization()
            .Produces(200)
            .ProducesProblem(401);

        // ── Detalle de una clase ─────────────────────────────────────────────
        app.MapGet("/api/v1/classes/{id:guid}",
            async (Guid id, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new GetGymClassByIdQuery(id), ct);
                return Results.Ok(result);
            })
            .WithTags("GymClasses")
            .WithSummary("Detalle de una clase con sus horarios")
            .RequireAuthorization()
            .Produces(200)
            .ProducesProblem(404)
            .ProducesProblem(401);

        // ── Crear clase ──────────────────────────────────────────────────────
        app.MapPost("/api/v1/classes",
            async (CreateGymClassRequest body, ISender sender, CancellationToken ct) =>
            {
                var command = new CreateGymClassCommand(
                    body.Name, body.Description,
                    body.Category, body.Color,
                    body.DurationMinutes, body.MaxCapacity);

                var id = await sender.Send(command, ct);
                return Results.Created($"/api/v1/classes/{id}", new { id });
            })
            .WithTags("GymClasses")
            .WithSummary("Crear tipo de clase")
            .RequireAuthorization()
            .Produces(201)
            .ProducesProblem(400)
            .ProducesProblem(401);

        // ── Actualizar clase ─────────────────────────────────────────────────
        app.MapPut("/api/v1/classes/{id:guid}",
            async (Guid id, CreateGymClassRequest body, ISender sender, CancellationToken ct) =>
            {
                var command = new UpdateGymClassCommand(
                    id, body.Name, body.Description,
                    body.Category, body.Color,
                    body.DurationMinutes, body.MaxCapacity);

                await sender.Send(command, ct);
                return Results.NoContent();
            })
            .WithTags("GymClasses")
            .WithSummary("Actualizar tipo de clase")
            .RequireAuthorization()
            .Produces(204)
            .ProducesProblem(404)
            .ProducesProblem(400)
            .ProducesProblem(401);

        // ── Desactivar clase ─────────────────────────────────────────────────
        app.MapPatch("/api/v1/classes/{id:guid}/deactivate",
            async (Guid id, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new DeactivateGymClassCommand(id), ct);
                return Results.NoContent();
            })
            .WithTags("GymClasses")
            .WithSummary("Desactivar clase")
            .RequireAuthorization()
            .Produces(204)
            .ProducesProblem(404)
            .ProducesProblem(422)
            .ProducesProblem(401);

        // ── Reactivar clase ──────────────────────────────────────────────────
        app.MapPatch("/api/v1/classes/{id:guid}/reactivate",
            async (Guid id, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new ReactivateGymClassCommand(id), ct);
                return Results.NoContent();
            })
            .WithTags("GymClasses")
            .WithSummary("Reactivar clase")
            .RequireAuthorization()
            .Produces(204)
            .ProducesProblem(404)
            .ProducesProblem(422)
            .ProducesProblem(401);

        // ── Calendario semanal ───────────────────────────────────────────────
        app.MapGet("/api/v1/classes/schedule/weekly",
            async (ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new GetWeeklyScheduleQuery(), ct);
                return Results.Ok(result);
            })
            .WithTags("GymClasses")
            .WithSummary("Calendario semanal de horarios activos")
            .WithDescription("Devuelve todos los horarios activos de clases activas, ordenados por día y hora.")
            .RequireAuthorization()
            .Produces(200)
            .ProducesProblem(401);

        // ── Crear horario ────────────────────────────────────────────────────
        app.MapPost("/api/v1/classes/{classId:guid}/schedules",
            async (Guid classId, CreateScheduleRequest body, ISender sender, CancellationToken ct) =>
            {
                var command = new CreateClassScheduleCommand(
                    classId, body.DayOfWeek,
                    body.StartTime, body.EndTime,
                    body.InstructorName, body.Room, body.MaxCapacity);

                var id = await sender.Send(command, ct);
                return Results.Created($"/api/v1/classes/{classId}/schedules/{id}", new { id });
            })
            .WithTags("GymClasses")
            .WithSummary("Crear horario para una clase")
            .RequireAuthorization()
            .Produces(201)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(422)
            .ProducesProblem(401);

        // ── Actualizar horario ───────────────────────────────────────────────
        app.MapPut("/api/v1/schedules/{id:guid}",
            async (Guid id, CreateScheduleRequest body, ISender sender, CancellationToken ct) =>
            {
                var command = new UpdateClassScheduleCommand(
                    id, body.DayOfWeek,
                    body.StartTime, body.EndTime,
                    body.InstructorName, body.Room, body.MaxCapacity);

                await sender.Send(command, ct);
                return Results.NoContent();
            })
            .WithTags("GymClasses")
            .WithSummary("Actualizar horario")
            .RequireAuthorization()
            .Produces(204)
            .ProducesProblem(404)
            .ProducesProblem(400)
            .ProducesProblem(401);

        // ── Eliminar horario ─────────────────────────────────────────────────
        app.MapDelete("/api/v1/schedules/{id:guid}",
            async (Guid id, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new DeleteClassScheduleCommand(id), ct);
                return Results.NoContent();
            })
            .WithTags("GymClasses")
            .WithSummary("Eliminar horario (soft delete)")
            .RequireAuthorization()
            .Produces(204)
            .ProducesProblem(404)
            .ProducesProblem(401);

        return app;
    }
}

// ── Request bodies ────────────────────────────────────────────────────────────

public sealed record CreateGymClassRequest(
    string Name,
    string? Description,
    ClassCategory Category,
    string? Color,
    int DurationMinutes,
    int MaxCapacity
);

public sealed record CreateScheduleRequest(
    int DayOfWeek,
    string StartTime,
    string EndTime,
    string? InstructorName,
    string? Room,
    int? MaxCapacity
);
