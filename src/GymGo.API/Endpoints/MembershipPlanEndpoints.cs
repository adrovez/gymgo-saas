using GymGo.Application.MembershipPlans.Commands.CreateMembershipPlan;
using GymGo.Application.MembershipPlans.Commands.DeactivateMembershipPlan;
using GymGo.Application.MembershipPlans.Commands.ReactivateMembershipPlan;
using GymGo.Application.MembershipPlans.Commands.UpdateMembershipPlan;
using GymGo.Application.MembershipPlans.Queries.GetMembershipPlanById;
using GymGo.Application.MembershipPlans.Queries.GetMembershipPlans;
using GymGo.Domain.MembershipPlans;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GymGo.API.Endpoints;

public static class MembershipPlanEndpoints
{
    public static IEndpointRouteBuilder MapMembershipPlanEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/membership-plans")
            .WithTags("MembershipPlans")
            .RequireAuthorization();

        // ── GET /api/v1/membership-plans ─────────────────────────────────────
        group.MapGet("/", async (
            [FromQuery] string? search,
            [FromQuery] Periodicity? periodicity,
            [FromQuery] bool? isActive,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetMembershipPlansQuery(search, periodicity, isActive), ct);
            return Results.Ok(result);
        })
        .WithSummary("Listar planes de membresía")
        .WithDescription("Retorna todos los planes del gimnasio. Filtros opcionales: nombre, periodicidad y estado activo.")
        .Produces(200)
        .ProducesProblem(401);

        // ── GET /api/v1/membership-plans/{id} ────────────────────────────────
        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var dto = await sender.Send(new GetMembershipPlanByIdQuery(id), ct);
            return Results.Ok(dto);
        })
        .WithSummary("Obtener plan por Id")
        .WithDescription("Retorna el detalle completo de un plan. 404 si no existe en el tenant actual.")
        .Produces(200)
        .ProducesProblem(404)
        .ProducesProblem(401);

        // ── POST /api/v1/membership-plans ────────────────────────────────────
        group.MapPost("/", async (CreateMembershipPlanCommand command, ISender sender, CancellationToken ct) =>
        {
            var id = await sender.Send(command, ct);
            return Results.Created($"/api/v1/membership-plans/{id}", new { id });
        })
        .WithSummary("Crear plan de membresía")
        .WithDescription("Crea un nuevo plan. Si FixedDays=true, los días marcados deben coincidir con DaysPerWeek. Si FreeSchedule=false, TimeFrom y TimeTo son obligatorios.")
        .Produces(201)
        .ProducesProblem(400)
        .ProducesProblem(422)
        .ProducesProblem(401);

        // ── PUT /api/v1/membership-plans/{id} ────────────────────────────────
        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateMembershipPlanRequest body,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = new UpdateMembershipPlanCommand(
                PlanId:         id,
                Name:           body.Name,
                Description:    body.Description,
                Periodicity:    body.Periodicity,
                DaysPerWeek:    body.DaysPerWeek,
                FixedDays:      body.FixedDays,
                Monday:         body.Monday,
                Tuesday:        body.Tuesday,
                Wednesday:      body.Wednesday,
                Thursday:       body.Thursday,
                Friday:         body.Friday,
                Saturday:       body.Saturday,
                Sunday:         body.Sunday,
                FreeSchedule:   body.FreeSchedule,
                TimeFrom:       body.TimeFrom,
                TimeTo:         body.TimeTo,
                Amount:         body.Amount,
                AllowsFreezing: body.AllowsFreezing);

            await sender.Send(command, ct);
            return Results.NoContent();
        })
        .WithSummary("Actualizar plan")
        .Produces(204)
        .ProducesProblem(400)
        .ProducesProblem(404)
        .ProducesProblem(422)
        .ProducesProblem(401);

        // ── PATCH /api/v1/membership-plans/{id}/deactivate ───────────────────
        group.MapPatch("/{id:guid}/deactivate", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new DeactivateMembershipPlanCommand(id), ct);
            return Results.NoContent();
        })
        .WithSummary("Desactivar plan")
        .WithDescription("El plan deja de estar disponible para nuevas asignaciones. Los socios actuales no se ven afectados.")
        .Produces(204)
        .ProducesProblem(404)
        .ProducesProblem(422)
        .ProducesProblem(401);

        // ── PATCH /api/v1/membership-plans/{id}/reactivate ───────────────────
        group.MapPatch("/{id:guid}/reactivate", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new ReactivateMembershipPlanCommand(id), ct);
            return Results.NoContent();
        })
        .WithSummary("Reactivar plan")
        .Produces(204)
        .ProducesProblem(404)
        .ProducesProblem(401);

        return app;
    }
}

// ── Request body ─────────────────────────────────────────────────────────────

/// <summary>Body para PUT /membership-plans/{id}.</summary>
public sealed record UpdateMembershipPlanRequest(
    string Name,
    string? Description,
    Periodicity Periodicity,
    int DaysPerWeek,
    bool FixedDays,
    bool Monday,
    bool Tuesday,
    bool Wednesday,
    bool Thursday,
    bool Friday,
    bool Saturday,
    bool Sunday,
    bool FreeSchedule,
    TimeOnly? TimeFrom,
    TimeOnly? TimeTo,
    decimal Amount,
    bool AllowsFreezing
);
