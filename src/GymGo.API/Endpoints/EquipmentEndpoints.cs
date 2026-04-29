using GymGo.Application.Equipment.Commands.CreateEquipment;
using GymGo.Application.Equipment.Commands.DeactivateEquipment;
using GymGo.Application.Equipment.Commands.ReactivateEquipment;
using GymGo.Application.Equipment.Commands.UpdateEquipment;
using GymGo.Application.Equipment.Queries.GetEquipmentById;
using GymGo.Application.Equipment.Queries.GetEquipments;
using MediatR;

namespace GymGo.API.Endpoints;

public static class EquipmentEndpoints
{
    public static IEndpointRouteBuilder MapEquipmentEndpoints(this IEndpointRouteBuilder app)
    {
        // ── Listar máquinas ───────────────────────────────────────────────
        // GET /api/v1/equipment?isActive=true
        app.MapGet("/api/v1/equipment",
            async (bool? isActive, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new GetEquipmentsQuery(isActive), ct);
                return Results.Ok(result);
            })
            .WithTags("Equipment")
            .WithSummary("Listar maquinaria")
            .WithDescription(
                "Devuelve el catálogo de máquinas del tenant. " +
                "isActive=true devuelve solo activas, false solo inactivas, sin filtro devuelve todas.")
            .RequireAuthorization()
            .Produces(200)
            .ProducesProblem(401);

        // ── Obtener máquina por Id ─────────────────────────────────────────
        // GET /api/v1/equipment/{id}
        app.MapGet("/api/v1/equipment/{id:guid}",
            async (Guid id, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new GetEquipmentByIdQuery(id), ct);
                return Results.Ok(result);
            })
            .WithTags("Equipment")
            .WithSummary("Obtener máquina por Id")
            .RequireAuthorization()
            .Produces(200)
            .ProducesProblem(404)
            .ProducesProblem(401);

        // ── Crear máquina ─────────────────────────────────────────────────
        // POST /api/v1/equipment
        // Body: { name, brand?, model?, serialNumber?, purchaseDate?, imageUrl? }
        app.MapPost("/api/v1/equipment",
            async (CreateEquipmentRequest body, ISender sender, CancellationToken ct) =>
            {
                var command = new CreateEquipmentCommand(
                    Name:         body.Name,
                    Brand:        body.Brand,
                    Model:        body.Model,
                    SerialNumber: body.SerialNumber,
                    PurchaseDate: body.PurchaseDate,
                    ImageUrl:     body.ImageUrl);

                var id = await sender.Send(command, ct);
                return Results.Created($"/api/v1/equipment/{id}", new { id });
            })
            .WithTags("Equipment")
            .WithSummary("Registrar nueva máquina")
            .WithDescription(
                "Registra una nueva máquina en el catálogo del tenant. " +
                "Solo el nombre es obligatorio.")
            .RequireAuthorization()
            .Produces(201)
            .ProducesProblem(400)
            .ProducesProblem(422)
            .ProducesProblem(401);

        // ── Actualizar máquina ────────────────────────────────────────────
        // PUT /api/v1/equipment/{id}
        app.MapPut("/api/v1/equipment/{id:guid}",
            async (Guid id, CreateEquipmentRequest body, ISender sender, CancellationToken ct) =>
            {
                var command = new UpdateEquipmentCommand(
                    Id:           id,
                    Name:         body.Name,
                    Brand:        body.Brand,
                    Model:        body.Model,
                    SerialNumber: body.SerialNumber,
                    PurchaseDate: body.PurchaseDate,
                    ImageUrl:     body.ImageUrl);

                await sender.Send(command, ct);
                return Results.NoContent();
            })
            .WithTags("Equipment")
            .WithSummary("Actualizar datos de una máquina")
            .RequireAuthorization()
            .Produces(204)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(422)
            .ProducesProblem(401);

        // ── Desactivar máquina ────────────────────────────────────────────
        // POST /api/v1/equipment/{id}/deactivate
        app.MapPost("/api/v1/equipment/{id:guid}/deactivate",
            async (Guid id, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new DeactivateEquipmentCommand(id), ct);
                return Results.NoContent();
            })
            .WithTags("Equipment")
            .WithSummary("Desactivar una máquina")
            .WithDescription("Marca la máquina como inactiva. Retorna 422 si ya está inactiva.")
            .RequireAuthorization()
            .Produces(204)
            .ProducesProblem(404)
            .ProducesProblem(422)
            .ProducesProblem(401);

        // ── Reactivar máquina ─────────────────────────────────────────────
        // POST /api/v1/equipment/{id}/reactivate
        app.MapPost("/api/v1/equipment/{id:guid}/reactivate",
            async (Guid id, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new ReactivateEquipmentCommand(id), ct);
                return Results.NoContent();
            })
            .WithTags("Equipment")
            .WithSummary("Reactivar una máquina")
            .WithDescription("Reactiva una máquina previamente desactivada. Retorna 422 si ya está activa.")
            .RequireAuthorization()
            .Produces(204)
            .ProducesProblem(404)
            .ProducesProblem(422)
            .ProducesProblem(401);

        return app;
    }
}

// ── Request bodies ─────────────────────────────────────────────────────────────

/// <summary>Cuerpo del request para crear o actualizar una máquina.</summary>
public sealed record CreateEquipmentRequest(
    /// <summary>Nombre de la máquina (obligatorio).</summary>
    string    Name,
    /// <summary>Marca del equipo.</summary>
    string?   Brand,
    /// <summary>Modelo del equipo.</summary>
    string?   Model,
    /// <summary>Número de serie del fabricante.</summary>
    string?   SerialNumber,
    /// <summary>Fecha de compra.</summary>
    DateOnly? PurchaseDate,
    /// <summary>URL de foto de la máquina.</summary>
    string?   ImageUrl
);
