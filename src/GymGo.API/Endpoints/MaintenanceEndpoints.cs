using GymGo.Application.Maintenance.Commands.CancelMaintenance;
using GymGo.Application.Maintenance.Commands.CompleteMaintenance;
using GymGo.Application.Maintenance.Commands.CreateMaintenanceRecord;
using GymGo.Application.Maintenance.Commands.StartMaintenance;
using GymGo.Application.Maintenance.Queries.GetMaintenanceRecordById;
using GymGo.Application.Maintenance.Queries.GetMaintenanceRecords;
using GymGo.Domain.Maintenance;
using MediatR;

namespace GymGo.API.Endpoints;

public static class MaintenanceEndpoints
{
    public static IEndpointRouteBuilder MapMaintenanceEndpoints(this IEndpointRouteBuilder app)
    {
        // ── Listar registros de mantención ────────────────────────────────
        // GET /api/v1/maintenance?equipmentId=...&type=0&status=0
        app.MapGet("/api/v1/maintenance",
            async (Guid? equipmentId, int? type, int? status,
                   ISender sender, CancellationToken ct) =>
            {
                var typeEnum   = type.HasValue   ? (MaintenanceType?)type.Value   : null;
                var statusEnum = status.HasValue ? (MaintenanceStatus?)status.Value : null;

                var result = await sender.Send(
                    new GetMaintenanceRecordsQuery(equipmentId, typeEnum, statusEnum), ct);
                return Results.Ok(result);
            })
            .WithTags("Maintenance")
            .WithSummary("Listar registros de mantención")
            .WithDescription(
                "Devuelve registros de mantención del tenant. Filtros opcionales: " +
                "equipmentId, type (0=Preventiva, 1=Correctiva), " +
                "status (0=Pendiente, 1=EnProceso, 2=Completada, 3=Cancelada).")
            .RequireAuthorization()
            .Produces(200)
            .ProducesProblem(401);

        // ── Obtener registro por Id ────────────────────────────────────────
        // GET /api/v1/maintenance/{id}
        app.MapGet("/api/v1/maintenance/{id:guid}",
            async (Guid id, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new GetMaintenanceRecordByIdQuery(id), ct);
                return Results.Ok(result);
            })
            .WithTags("Maintenance")
            .WithSummary("Obtener registro de mantención por Id")
            .RequireAuthorization()
            .Produces(200)
            .ProducesProblem(404)
            .ProducesProblem(401);

        // ── Registros de una máquina ──────────────────────────────────────
        // GET /api/v1/equipment/{equipmentId}/maintenance?type=0&status=0
        app.MapGet("/api/v1/equipment/{equipmentId:guid}/maintenance",
            async (Guid equipmentId, int? type, int? status,
                   ISender sender, CancellationToken ct) =>
            {
                var typeEnum   = type.HasValue   ? (MaintenanceType?)type.Value   : null;
                var statusEnum = status.HasValue ? (MaintenanceStatus?)status.Value : null;

                var result = await sender.Send(
                    new GetMaintenanceRecordsQuery(equipmentId, typeEnum, statusEnum), ct);
                return Results.Ok(result);
            })
            .WithTags("Maintenance")
            .WithSummary("Historial de mantención de una máquina")
            .WithDescription("Devuelve todos los registros de mantención de la máquina indicada.")
            .RequireAuthorization()
            .Produces(200)
            .ProducesProblem(401);

        // ── Crear registro de mantención ──────────────────────────────────
        // POST /api/v1/maintenance
        app.MapPost("/api/v1/maintenance",
            async (CreateMaintenanceRecordRequest body, ISender sender, CancellationToken ct) =>
            {
                var command = new CreateMaintenanceRecordCommand(
                    EquipmentId:            body.EquipmentId,
                    Type:                   body.Type,
                    ScheduledDate:          body.ScheduledDate,
                    Description:            body.Description,
                    ResponsibleType:        body.ResponsibleType,
                    ResponsibleUserId:      body.ResponsibleUserId,
                    ExternalProviderName:   body.ExternalProviderName,
                    ExternalProviderContact: body.ExternalProviderContact);

                var id = await sender.Send(command, ct);
                return Results.Created($"/api/v1/maintenance/{id}", new { id });
            })
            .WithTags("Maintenance")
            .WithSummary("Registrar mantención")
            .WithDescription(
                "Crea un registro de mantención en estado Pendiente. " +
                "Para responsable interno (0) se requiere responsibleUserId. " +
                "Para responsable externo (1) se requiere externalProviderName.")
            .RequireAuthorization()
            .Produces(201)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(422)
            .ProducesProblem(401);

        // ── Iniciar mantención ────────────────────────────────────────────
        // POST /api/v1/maintenance/{id}/start
        app.MapPost("/api/v1/maintenance/{id:guid}/start",
            async (Guid id, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new StartMaintenanceCommand(id), ct);
                return Results.NoContent();
            })
            .WithTags("Maintenance")
            .WithSummary("Iniciar mantención")
            .WithDescription(
                "Cambia el estado de Pendiente a En Proceso. " +
                "Retorna 422 si la mantención no está en estado Pendiente.")
            .RequireAuthorization()
            .Produces(204)
            .ProducesProblem(404)
            .ProducesProblem(422)
            .ProducesProblem(401);

        // ── Completar mantención ──────────────────────────────────────────
        // POST /api/v1/maintenance/{id}/complete
        // Body: { notes?, cost? }
        app.MapPost("/api/v1/maintenance/{id:guid}/complete",
            async (Guid id, CompleteMaintenanceRequest body, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(
                    new CompleteMaintenanceCommand(id, body.Notes, body.Cost), ct);
                return Results.NoContent();
            })
            .WithTags("Maintenance")
            .WithSummary("Completar mantención")
            .WithDescription(
                "Cambia el estado de En Proceso a Completada. " +
                "Se pueden registrar observaciones y costo. " +
                "Retorna 422 si la mantención no está En Proceso.")
            .RequireAuthorization()
            .Produces(204)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(422)
            .ProducesProblem(401);

        // ── Cancelar mantención ───────────────────────────────────────────
        // POST /api/v1/maintenance/{id}/cancel?reason=...
        app.MapPost("/api/v1/maintenance/{id:guid}/cancel",
            async (Guid id, string? reason, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new CancelMaintenanceCommand(id, reason), ct);
                return Results.NoContent();
            })
            .WithTags("Maintenance")
            .WithSummary("Cancelar mantención")
            .WithDescription(
                "Cancela una mantención Pendiente o En Proceso. " +
                "El motivo se puede indicar como query param reason. " +
                "Retorna 422 si la mantención ya está Completada o Cancelada.")
            .RequireAuthorization()
            .Produces(204)
            .ProducesProblem(404)
            .ProducesProblem(422)
            .ProducesProblem(401);

        return app;
    }
}

// ── Request bodies ─────────────────────────────────────────────────────────────

/// <summary>Cuerpo del request para crear un registro de mantención.</summary>
public sealed record CreateMaintenanceRecordRequest(
    /// <summary>Id de la máquina a mantener.</summary>
    Guid            EquipmentId,
    /// <summary>Tipo: 0=Preventiva, 1=Correctiva.</summary>
    MaintenanceType Type,
    /// <summary>Fecha programada de la mantención.</summary>
    DateOnly        ScheduledDate,
    /// <summary>Descripción del trabajo a realizar.</summary>
    string          Description,
    /// <summary>Responsable: 0=Interno, 1=Externo.</summary>
    ResponsibleType ResponsibleType,
    /// <summary>Id del usuario interno responsable (requerido si ResponsibleType=0).</summary>
    Guid?           ResponsibleUserId,
    /// <summary>Nombre del proveedor externo (requerido si ResponsibleType=1).</summary>
    string?         ExternalProviderName,
    /// <summary>Teléfono o email del proveedor externo.</summary>
    string?         ExternalProviderContact
);

/// <summary>Cuerpo del request para completar una mantención.</summary>
public sealed record CompleteMaintenanceRequest(
    /// <summary>Observaciones de cierre (opcional).</summary>
    string?  Notes,
    /// <summary>Costo incurrido en la mantención (opcional).</summary>
    decimal? Cost
);
