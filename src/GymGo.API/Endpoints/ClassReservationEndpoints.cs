using GymGo.Application.ClassReservations.Commands.CancelReservation;
using GymGo.Application.ClassReservations.Commands.CreateReservation;
using GymGo.Application.ClassReservations.Queries.GetReservationsByMember;
using GymGo.Application.ClassReservations.Queries.GetReservationsBySession;
using GymGo.Domain.ClassReservations;
using MediatR;

namespace GymGo.API.Endpoints;

public static class ClassReservationEndpoints
{
    public static IEndpointRouteBuilder MapClassReservationEndpoints(this IEndpointRouteBuilder app)
    {
        // ── Crear reserva ────────────────────────────────────────────────────
        // POST /api/v1/reservations
        // Body: { memberId, classScheduleId, sessionDate, notes? }
        app.MapPost("/api/v1/reservations",
            async (CreateReservationRequest body, ISender sender, CancellationToken ct) =>
            {
                var command = new CreateReservationCommand(
                    MemberId:        body.MemberId,
                    ClassScheduleId: body.ClassScheduleId,
                    SessionDate:     body.SessionDate,
                    Notes:           body.Notes);

                var id = await sender.Send(command, ct);
                return Results.Created($"/api/v1/reservations/{id}", new { id });
            })
            .WithTags("Reservations")
            .WithSummary("Crear reserva para una sesión de clase")
            .WithDescription(
                "Reserva un cupo para el socio en la sesión indicada (ClassScheduleId + SessionDate). " +
                "Valida que el socio esté activo, que el horario esté activo, que la fecha " +
                "coincida con el día del horario, que no haya reserva duplicada y que " +
                "existan cupos disponibles. Retorna 422 si alguna validación falla.")
            .RequireAuthorization()
            .Produces(201)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(422)
            .ProducesProblem(401);

        // ── Cancelar reserva ─────────────────────────────────────────────────
        // DELETE /api/v1/reservations/{id}?cancelStatus=1&reason=...
        app.MapDelete("/api/v1/reservations/{id:guid}",
            async (Guid id, ReservationStatus? cancelStatus, string? reason,
                   ISender sender, CancellationToken ct) =>
            {
                var command = new CancelReservationCommand(
                    ReservationId: id,
                    CancelStatus:  cancelStatus ?? ReservationStatus.CancelledByMember,
                    Reason:        reason);

                await sender.Send(command, ct);
                return Results.NoContent();
            })
            .WithTags("Reservations")
            .WithSummary("Cancelar una reserva")
            .WithDescription(
                "Cancela una reserva activa. " +
                "cancelStatus puede ser 1 (CancelledByMember) o 2 (CancelledByStaff). " +
                "Parámetros opcionales vía query string: cancelStatus, reason. " +
                "Retorna 422 si la reserva ya está cancelada o completada.")
            .RequireAuthorization()
            .Produces(204)
            .ProducesProblem(404)
            .ProducesProblem(422)
            .ProducesProblem(401);

        // ── Reservas de una sesión ───────────────────────────────────────────
        // GET /api/v1/schedules/{scheduleId}/reservations?sessionDate=YYYY-MM-DD
        app.MapGet("/api/v1/schedules/{scheduleId:guid}/reservations",
            async (Guid scheduleId, DateOnly sessionDate, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(
                    new GetReservationsBySessionQuery(scheduleId, sessionDate), ct);
                return Results.Ok(result);
            })
            .WithTags("Reservations")
            .WithSummary("Reservas de una sesión de clase")
            .WithDescription(
                "Devuelve todas las reservas (en cualquier estado) para la sesión indicada " +
                "(ClassScheduleId + SessionDate), ordenadas por fecha de reserva.")
            .RequireAuthorization()
            .Produces(200)
            .ProducesProblem(401);

        // ── Reservas de un socio ─────────────────────────────────────────────
        // GET /api/v1/members/{memberId}/reservations?status=0&from=YYYY-MM-DD&to=YYYY-MM-DD
        app.MapGet("/api/v1/members/{memberId:guid}/reservations",
            async (Guid memberId, int? status, DateOnly? from, DateOnly? to,
                   ISender sender, CancellationToken ct) =>
            {
                var statusEnum = status.HasValue
                    ? (ReservationStatus?)status.Value
                    : null;

                var result = await sender.Send(
                    new GetReservationsByMemberQuery(memberId, statusEnum, from, to), ct);
                return Results.Ok(result);
            })
            .WithTags("Reservations")
            .WithSummary("Reservas de un socio")
            .WithDescription(
                "Devuelve las reservas del socio, opcionalmente filtradas por estado y/o " +
                "rango de fechas de sesión. Ordenadas por SessionDate descendente.")
            .RequireAuthorization()
            .Produces(200)
            .ProducesProblem(401);

        return app;
    }
}

// ── Request bodies ─────────────────────────────────────────────────────────────

/// <summary>
/// Cuerpo del request para crear una reserva.
/// </summary>
public sealed record CreateReservationRequest(
    /// <summary>Id del socio.</summary>
    Guid MemberId,

    /// <summary>Id del horario semanal (ClassSchedule).</summary>
    Guid ClassScheduleId,

    /// <summary>Fecha exacta de la sesión a reservar.</summary>
    DateOnly SessionDate,

    /// <summary>Observaciones opcionales.</summary>
    string? Notes
);

