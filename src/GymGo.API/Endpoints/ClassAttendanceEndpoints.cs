using GymGo.Application.ClassAttendances.Commands.CheckInMember;
using GymGo.Application.ClassAttendances.Queries.GetAttendancesBySession;
using GymGo.Domain.ClassAttendances;
using MediatR;

namespace GymGo.API.Endpoints;

public static class ClassAttendanceEndpoints
{
    public static IEndpointRouteBuilder MapClassAttendanceEndpoints(this IEndpointRouteBuilder app)
    {
        // ── Check-in (manual o por QR) ───────────────────────────────────────
        // POST /api/v1/attendances
        // Body: { memberId, classScheduleId, sessionDate?, checkInMethod?, notes? }
        app.MapPost("/api/v1/attendances",
            async (CheckInRequest body, ISender sender, CancellationToken ct) =>
            {
                var command = new CheckInMemberCommand(
                    MemberId:        body.MemberId,
                    ClassScheduleId: body.ClassScheduleId,
                    SessionDate:     body.SessionDate,
                    CheckInMethod:   body.CheckInMethod ?? CheckInMethod.Manual,
                    Notes:           body.Notes);

                var id = await sender.Send(command, ct);
                return Results.Created($"/api/v1/attendances/{id}", new { id });
            })
            .WithTags("Attendances")
            .WithSummary("Registrar asistencia (check-in manual o QR)")
            .WithDescription(
                "Registra el check-in de un socio a una sesión concreta de clase. " +
                "Si no se indica sessionDate, se usa la fecha actual (UTC). " +
                "Retorna 422 si el socio ya tiene check-in para esa sesión.")
            .RequireAuthorization()
            .Produces(201)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(422)
            .ProducesProblem(401);

        // ── Asistentes de una sesión ─────────────────────────────────────────
        // GET /api/v1/schedules/{scheduleId}/attendances?sessionDate=2025-04-28
        app.MapGet("/api/v1/schedules/{scheduleId:guid}/attendances",
            async (Guid scheduleId, DateOnly? sessionDate, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(
                    new GetAttendancesBySessionQuery(scheduleId, sessionDate), ct);
                return Results.Ok(result);
            })
            .WithTags("Attendances")
            .WithSummary("Asistentes de una sesión de clase")
            .WithDescription(
                "Devuelve todos los check-ins de un horario para la fecha indicada. " +
                "Si no se indica sessionDate, se usa la fecha actual (UTC).")
            .RequireAuthorization()
            .Produces(200)
            .ProducesProblem(401);

        return app;
    }
}

// ── Request bodies ─────────────────────────────────────────────────────────────

/// <summary>
/// Cuerpo del request para registrar un check-in.
/// </summary>
public sealed record CheckInRequest(
    /// <summary>Id del socio.</summary>
    Guid MemberId,

    /// <summary>Id del horario semanal (ClassSchedule).</summary>
    Guid ClassScheduleId,

    /// <summary>Fecha de la sesión. Si es null se usa la fecha UTC actual.</summary>
    DateOnly? SessionDate,

    /// <summary>Método de check-in: 0 = Manual (default), 1 = QR.</summary>
    CheckInMethod? CheckInMethod,

    /// <summary>Observaciones opcionales de la recepcionista.</summary>
    string? Notes
);
