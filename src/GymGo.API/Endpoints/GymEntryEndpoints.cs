using GymGo.Application.GymEntries.Commands.RegisterGymEntry;
using GymGo.Application.GymEntries.Queries.GetGymEntriesByDate;
using GymGo.Domain.GymEntries;
using MediatR;

namespace GymGo.API.Endpoints;

public static class GymEntryEndpoints
{
    public static IEndpointRouteBuilder MapGymEntryEndpoints(this IEndpointRouteBuilder app)
    {
        // ── Registrar ingreso al gym ──────────────────────────────────────────
        // POST /api/v1/gym-entries
        // Body: { memberId, method?, notes? }
        app.MapPost("/api/v1/gym-entries",
            async (RegisterGymEntryRequest body, ISender sender, CancellationToken ct) =>
            {
                var command = new RegisterGymEntryCommand(
                    MemberId: body.MemberId,
                    Method:   body.Method ?? GymEntryMethod.Manual,
                    Notes:    body.Notes);

                var id = await sender.Send(command, ct);
                return Results.Created($"/api/v1/gym-entries/{id}", new { id });
            })
            .WithTags("GymEntries")
            .WithSummary("Registrar ingreso al gimnasio")
            .WithDescription(
                "Registra el ingreso de un socio al gimnasio. " +
                "Valida que la membresía esté activa, que el día de la semana esté habilitado " +
                "en el plan (si el plan tiene días fijos) y que el horario sea el correcto " +
                "(si el plan tiene horario restringido). " +
                "Retorna 422 si alguna validación falla.")
            .RequireAuthorization()
            .Produces(201)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(422)
            .ProducesProblem(401);

        // ── Ingresos por fecha ────────────────────────────────────────────────
        // GET /api/v1/gym-entries?date=2026-04-28
        app.MapGet("/api/v1/gym-entries",
            async (DateOnly? date, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new GetGymEntriesByDateQuery(date), ct);
                return Results.Ok(result);
            })
            .WithTags("GymEntries")
            .WithSummary("Ingresos del gimnasio por fecha")
            .WithDescription(
                "Devuelve todos los ingresos registrados para la fecha indicada, " +
                "ordenados del más reciente al más antiguo. " +
                "Si no se indica date, se usa la fecha actual (UTC).")
            .RequireAuthorization()
            .Produces(200)
            .ProducesProblem(401);

        return app;
    }
}

// ── Request bodies ─────────────────────────────────────────────────────────────

/// <summary>
/// Cuerpo del request para registrar el ingreso de un socio.
/// </summary>
public sealed record RegisterGymEntryRequest(
    /// <summary>Id del socio que ingresa al gimnasio.</summary>
    Guid MemberId,

    /// <summary>Método de ingreso: 0 = Manual (default), 1 = QR, 2 = Badge.</summary>
    GymEntryMethod? Method,

    /// <summary>Observaciones opcionales de la recepcionista.</summary>
    string? Notes
);
