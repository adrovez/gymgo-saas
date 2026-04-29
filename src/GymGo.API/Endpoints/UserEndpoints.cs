using GymGo.Application.Users.Commands.ChangeUserPassword;
using GymGo.Application.Users.Commands.CreateUser;
using GymGo.Application.Users.Commands.ToggleUserActive;
using GymGo.Application.Users.Commands.UpdateUser;
using GymGo.Application.Users.Queries.GetUserById;
using GymGo.Application.Users.Queries.GetUsers;
using GymGo.Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GymGo.API.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/users")
            .WithTags("Users")
            .RequireAuthorization();

        // ── GET /api/v1/users ────────────────────────────────────────────────
        group.MapGet("/", async (
            [FromQuery] string? search,
            [FromQuery] UserRole? role,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetUsersQuery(search, role), ct);
            return Results.Ok(result);
        })
        .WithSummary("Listar usuarios")
        .WithDescription("Retorna los usuarios del gimnasio actual. Soporta búsqueda por nombre/email y filtro por rol.")
        .Produces<GetUsersResult>()
        .ProducesProblem(401);

        // ── GET /api/v1/users/{id} ───────────────────────────────────────────
        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var dto = await sender.Send(new GetUserByIdQuery(id), ct);
            return Results.Ok(dto);
        })
        .WithSummary("Obtener usuario por Id")
        .WithDescription("Retorna el detalle de un usuario. Retorna 404 si no existe o no pertenece al tenant actual.")
        .Produces(200)
        .ProducesProblem(404)
        .ProducesProblem(401);

        // ── POST /api/v1/users ───────────────────────────────────────────────
        group.MapPost("/", async (CreateUserCommand command, ISender sender, CancellationToken ct) =>
        {
            var id = await sender.Send(command, ct);
            return Results.Created($"/api/v1/users/{id}", new { id });
        })
        .WithSummary("Crear usuario")
        .WithDescription("Crea un nuevo usuario (Staff o Instructor) en el gimnasio. El email debe ser único dentro del tenant.")
        .Produces(201)
        .ProducesProblem(400)
        .ProducesProblem(422)
        .ProducesProblem(401);

        // ── PUT /api/v1/users/{id} ───────────────────────────────────────────
        group.MapPut("/{id:guid}", async (Guid id, UpdateUserRequest body, ISender sender, CancellationToken ct) =>
        {
            var command = new UpdateUserCommand(
                UserId:   id,
                FullName: body.FullName,
                Role:     body.Role,
                IsActive: body.IsActive);

            await sender.Send(command, ct);
            return Results.NoContent();
        })
        .WithSummary("Actualizar usuario")
        .WithDescription("Actualiza nombre, rol y estado activo de un usuario.")
        .Produces(204)
        .ProducesProblem(400)
        .ProducesProblem(404)
        .ProducesProblem(401);

        // ── PATCH /api/v1/users/{id}/toggle-active ───────────────────────────
        group.MapPatch("/{id:guid}/toggle-active", async (Guid id, ToggleActiveRequest body, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new ToggleUserActiveCommand(id, body.IsActive), ct);
            return Results.NoContent();
        })
        .WithSummary("Activar / Desactivar usuario")
        .WithDescription("Cambia el estado activo de un usuario sin modificar el resto de sus datos.")
        .Produces(204)
        .ProducesProblem(404)
        .ProducesProblem(401);

        // ── PATCH /api/v1/users/{id}/password ───────────────────────────────
        group.MapPatch("/{id:guid}/password", async (Guid id, ChangePasswordRequest body, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new ChangeUserPasswordCommand(id, body.NewPassword), ct);
            return Results.NoContent();
        })
        .WithSummary("Cambiar contraseña")
        .WithDescription("Reemplaza la contraseña de un usuario con la nueva proporcionada.")
        .Produces(204)
        .ProducesProblem(400)
        .ProducesProblem(404)
        .ProducesProblem(401);

        return app;
    }
}

// ── Request bodies ───────────────────────────────────────────────────────────

/// <summary>Body para PUT /api/v1/users/{id}.</summary>
public sealed record UpdateUserRequest(string FullName, UserRole Role, bool IsActive);

/// <summary>Body para PATCH /api/v1/users/{id}/toggle-active.</summary>
public sealed record ToggleActiveRequest(bool IsActive);

/// <summary>Body para PATCH /api/v1/users/{id}/password.</summary>
public sealed record ChangePasswordRequest(string NewPassword);
