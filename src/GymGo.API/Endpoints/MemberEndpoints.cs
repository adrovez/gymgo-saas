using GymGo.Application.Members.Commands.ChangeMemberStatus;
using GymGo.Application.Members.Commands.CreateMember;
using GymGo.Application.Members.Commands.DeleteMember;
using GymGo.Application.Members.Commands.UpdateMember;
using GymGo.Application.Members.Queries.GetMemberById;
using GymGo.Application.Members.Queries.GetMembers;
using GymGo.Domain.Members;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GymGo.API.Endpoints;

public static class MemberEndpoints
{
    public static IEndpointRouteBuilder MapMemberEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/members")
            .WithTags("Members")
            .RequireAuthorization();

        // ── GET /api/v1/members ──────────────────────────────────────────────
        group.MapGet("/", async (
            [FromQuery] string? search,
            [FromQuery] MemberStatus? status,
            [FromQuery] int page,
            [FromQuery] int pageSize,
            ISender sender,
            CancellationToken ct) =>
        {
            var query = new GetMembersQuery(search, status, page <= 0 ? 1 : page, pageSize <= 0 ? 20 : pageSize);
            var result = await sender.Send(query, ct);
            return Results.Ok(result);
        })
        .WithSummary("Listar socios")
        .WithDescription("Retorna la lista paginada de socios del gimnasio actual. Soporta búsqueda por nombre, apellido o RUT y filtro por estado.")
        .Produces<GetMembersResult>()
        .ProducesProblem(401);

        // ── GET /api/v1/members/{id} ─────────────────────────────────────────
        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var dto = await sender.Send(new GetMemberByIdQuery(id), ct);
            return Results.Ok(dto);
        })
        .WithSummary("Obtener socio por Id")
        .WithDescription("Retorna el detalle completo de un socio. Retorna 404 si no existe o no pertenece al tenant actual.")
        .Produces(200)
        .ProducesProblem(404)
        .ProducesProblem(401);

        // ── POST /api/v1/members ─────────────────────────────────────────────
        group.MapPost("/", async (CreateMemberCommand command, ISender sender, CancellationToken ct) =>
        {
            var id = await sender.Send(command, ct);
            return Results.Created($"/api/v1/members/{id}", new { id });
        })
        .WithSummary("Crear socio")
        .WithDescription("Da de alta a un nuevo socio en el gimnasio. El RUT debe ser válido (módulo 11) y único dentro del gimnasio.")
        .Produces(201)
        .ProducesProblem(400)
        .ProducesProblem(422)
        .ProducesProblem(401);

        // ── PUT /api/v1/members/{id} ─────────────────────────────────────────
        group.MapPut("/{id:guid}", async (Guid id, UpdateMemberRequest body, ISender sender, CancellationToken ct) =>
        {
            var command = new UpdateMemberCommand(
                MemberId:               id,
                FirstName:              body.FirstName,
                LastName:               body.LastName,
                BirthDate:              body.BirthDate,
                Gender:                 body.Gender,
                Email:                  body.Email,
                Phone:                  body.Phone,
                Address:                body.Address,
                EmergencyContactName:   body.EmergencyContactName,
                EmergencyContactPhone:  body.EmergencyContactPhone,
                Notes:                  body.Notes);

            await sender.Send(command, ct);
            return Results.NoContent();
        })
        .WithSummary("Actualizar socio")
        .WithDescription("Actualiza los datos personales y de contacto de un socio. No modifica el RUT ni el estado.")
        .Produces(204)
        .ProducesProblem(400)
        .ProducesProblem(404)
        .ProducesProblem(422)
        .ProducesProblem(401);

        // ── PATCH /api/v1/members/{id}/status ───────────────────────────────
        group.MapPatch("/{id:guid}/status", async (
            Guid id,
            ChangeMemberStatusRequest body,
            ISender sender,
            CancellationToken ct) =>
        {
            await sender.Send(new ChangeMemberStatusCommand(id, body.NewStatus), ct);
            return Results.NoContent();
        })
        .WithSummary("Cambiar estado del socio")
        .WithDescription("Cambia el estado del socio: 0=Activo, 1=Suspendido, 2=Moroso.")
        .Produces(204)
        .ProducesProblem(400)
        .ProducesProblem(404)
        .ProducesProblem(422)
        .ProducesProblem(401);

        // ── DELETE /api/v1/members/{id} ──────────────────────────────────────
        group.MapDelete("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new DeleteMemberCommand(id), ct);
            return Results.NoContent();
        })
        .WithSummary("Dar de baja a un socio")
        .WithDescription("Realiza un soft-delete del socio. El registro se conserva en la base de datos con IsDeleted = true.")
        .Produces(204)
        .ProducesProblem(404)
        .ProducesProblem(401);

        return app;
    }
}

// ── Request bodies ────────────────────────────────────────────────────────────

/// <summary>Body para PUT /members/{id}.</summary>
public sealed record UpdateMemberRequest(
    string FirstName,
    string LastName,
    DateOnly BirthDate,
    Gender Gender,
    string? Email,
    string? Phone,
    string? Address,
    string? EmergencyContactName,
    string? EmergencyContactPhone,
    string? Notes
);

/// <summary>Body para PATCH /members/{id}/status.</summary>
public sealed record ChangeMemberStatusRequest(MemberStatus NewStatus);
