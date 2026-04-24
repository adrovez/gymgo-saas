using GymGo.Application.MembershipAssignments.Commands.AssignMembershipPlan;
using GymGo.Application.MembershipAssignments.Commands.CancelAssignment;
using GymGo.Application.MembershipAssignments.Commands.FreezeMembership;
using GymGo.Application.MembershipAssignments.Commands.MarkAssignmentOverdue;
using GymGo.Application.MembershipAssignments.Commands.RegisterPayment;
using GymGo.Application.MembershipAssignments.Commands.UnfreezeMembership;
using GymGo.Application.MembershipAssignments.Queries.GetActiveAssignment;
using GymGo.Application.MembershipAssignments.Queries.GetMemberAssignments;
using GymGo.Application.MembershipAssignments.Queries.GetOverdueAssignments;
using MediatR;

namespace GymGo.API.Endpoints;

public static class MembershipAssignmentEndpoints
{
    public static IEndpointRouteBuilder MapMembershipAssignmentEndpoints(this IEndpointRouteBuilder app)
    {
        // ── Historial de un socio ────────────────────────────────────────────
        app.MapGet("/api/v1/members/{memberId:guid}/assignments",
            async (Guid memberId, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new GetMemberAssignmentsQuery(memberId), ct);
                return Results.Ok(result);
            })
            .WithTags("MembershipAssignments")
            .WithSummary("Historial de membresías del socio")
            .RequireAuthorization()
            .Produces(200)
            .ProducesProblem(401);

        // ── Membresía activa de un socio ─────────────────────────────────────
        app.MapGet("/api/v1/members/{memberId:guid}/assignments/active",
            async (Guid memberId, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new GetActiveAssignmentQuery(memberId), ct);
                return result is null ? Results.NoContent() : Results.Ok(result);
            })
            .WithTags("MembershipAssignments")
            .WithSummary("Membresía activa del socio")
            .WithDescription("Retorna la membresía activa o congelada del socio. 204 si no tiene membresía vigente.")
            .RequireAuthorization()
            .Produces(200)
            .Produces(204)
            .ProducesProblem(401);

        // ── Asignar plan a socio ─────────────────────────────────────────────
        app.MapPost("/api/v1/members/{memberId:guid}/assignments",
            async (Guid memberId, AssignPlanRequest body, ISender sender, CancellationToken ct) =>
            {
                var command = new AssignMembershipPlanCommand(memberId, body.MembershipPlanId, body.StartDate, body.Notes);
                var id = await sender.Send(command, ct);
                return Results.Created($"/api/v1/members/{memberId}/assignments/{id}", new { id });
            })
            .WithTags("MembershipAssignments")
            .WithSummary("Asignar plan de membresía a un socio")
            .WithDescription("El socio no puede tener ya una membresía activa o congelada. El plan debe estar activo.")
            .RequireAuthorization()
            .Produces(201)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(422)
            .ProducesProblem(401);

        // ── Registrar pago ───────────────────────────────────────────────────
        app.MapPatch("/api/v1/assignments/{id:guid}/pay",
            async (Guid id, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new RegisterPaymentCommand(id), ct);
                return Results.NoContent();
            })
            .WithTags("MembershipAssignments")
            .WithSummary("Registrar pago de membresía")
            .WithDescription("Marca la membresía como pagada. Si el socio estaba moroso, se reactiva automáticamente a Activo.")
            .RequireAuthorization()
            .Produces(204)
            .ProducesProblem(404)
            .ProducesProblem(422)
            .ProducesProblem(401);

        // ── Marcar como morosa ───────────────────────────────────────────────
        app.MapPatch("/api/v1/assignments/{id:guid}/overdue",
            async (Guid id, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new MarkAssignmentOverdueCommand(id), ct);
                return Results.NoContent();
            })
            .WithTags("MembershipAssignments")
            .WithSummary("Marcar membresía como morosa")
            .WithDescription("Cambia el estado de pago a Overdue y marca al socio como Delinquent.")
            .RequireAuthorization()
            .Produces(204)
            .ProducesProblem(404)
            .ProducesProblem(422)
            .ProducesProblem(401);

        // ── Cancelar membresía ───────────────────────────────────────────────
        app.MapPatch("/api/v1/assignments/{id:guid}/cancel",
            async (Guid id, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new CancelAssignmentCommand(id), ct);
                return Results.NoContent();
            })
            .WithTags("MembershipAssignments")
            .WithSummary("Cancelar membresía")
            .RequireAuthorization()
            .Produces(204)
            .ProducesProblem(404)
            .ProducesProblem(422)
            .ProducesProblem(401);

        // ── Congelar membresía ───────────────────────────────────────────────
        app.MapPatch("/api/v1/assignments/{id:guid}/freeze",
            async (Guid id, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new FreezeMembershipCommand(id), ct);
                return Results.NoContent();
            })
            .WithTags("MembershipAssignments")
            .WithSummary("Congelar membresía")
            .WithDescription("Solo disponible si el plan tiene AllowsFreezing = true.")
            .RequireAuthorization()
            .Produces(204)
            .ProducesProblem(404)
            .ProducesProblem(422)
            .ProducesProblem(401);

        // ── Descongelar membresía ────────────────────────────────────────────
        app.MapPatch("/api/v1/assignments/{id:guid}/unfreeze",
            async (Guid id, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new UnfreezeMembershipCommand(id), ct);
                return Results.NoContent();
            })
            .WithTags("MembershipAssignments")
            .WithSummary("Descongelar membresía")
            .WithDescription("Extiende la fecha de vencimiento por los días que estuvo congelada.")
            .RequireAuthorization()
            .Produces(204)
            .ProducesProblem(404)
            .ProducesProblem(422)
            .ProducesProblem(401);

        // ── Listado de morosos del tenant ────────────────────────────────────
        app.MapGet("/api/v1/assignments/overdue",
            async (ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new GetOverdueAssignmentsQuery(), ct);
                return Results.Ok(result);
            })
            .WithTags("MembershipAssignments")
            .WithSummary("Listado de membresías morosas")
            .RequireAuthorization()
            .Produces(200)
            .ProducesProblem(401);

        return app;
    }
}

// ── Request bodies ────────────────────────────────────────────────────────────

public sealed record AssignPlanRequest(
    Guid MembershipPlanId,
    DateOnly? StartDate,
    string? Notes
);
