using GymGo.Application.Cash.Commands.RegisterTransaction;
using GymGo.Application.Cash.Commands.VoidTransaction;
using GymGo.Application.Cash.Queries.GetCashSummary;
using GymGo.Application.Cash.Queries.GetTransactions;
using GymGo.Domain.Cash;
using MediatR;

namespace GymGo.API.Endpoints;

public static class CashEndpoints
{
    public static IEndpointRouteBuilder MapCashEndpoints(this IEndpointRouteBuilder app)
    {
        // ── Registrar transacción (ingreso o egreso) ─────────────────────────
        app.MapPost("/api/v1/cash/transactions",
            async (RegisterCashRequest body, ISender sender, CancellationToken ct) =>
            {
                var command = new RegisterTransactionCommand(
                    body.Date,
                    body.Type,
                    body.Amount,
                    body.PaymentMethod,
                    body.Concept,
                    body.Description,
                    body.MemberId,
                    body.MembershipAssignmentId);

                var id = await sender.Send(command, ct);
                return Results.Created($"/api/v1/cash/transactions/{id}", new { id });
            })
            .WithTags("Cash")
            .WithSummary("Registrar transacción de caja")
            .WithDescription(
                "Registra un ingreso (cuota, matrícula, venta) o un egreso (servicios, mantención, insumos). " +
                "Los egresos requieren descripción. Los vínculos a socio o membresía son solo para ingresos.")
            .RequireAuthorization()
            .Produces(201)
            .ProducesProblem(400)
            .ProducesProblem(422)
            .ProducesProblem(401);

        // ── Historial de transacciones ───────────────────────────────────────
        app.MapGet("/api/v1/cash/transactions",
            async (
                DateOnly from, DateOnly to,
                CashTransactionType? type,
                Guid? memberId,
                ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new GetTransactionsQuery(from, to, type, memberId), ct);
                return Results.Ok(result);
            })
            .WithTags("Cash")
            .WithSummary("Historial de transacciones de caja")
            .WithDescription("Incluye ingresos y egresos, incluyendo los anulados. Filtrar por 'type' para ver solo ingresos o solo egresos.")
            .RequireAuthorization()
            .Produces(200)
            .ProducesProblem(401);

        // ── Resumen del período ──────────────────────────────────────────────
        app.MapGet("/api/v1/cash/summary",
            async (DateOnly from, DateOnly to, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new GetCashSummaryQuery(from, to), ct);
                return Results.Ok(result);
            })
            .WithTags("Cash")
            .WithSummary("Resumen de caja por período")
            .WithDescription(
                "Retorna totales de ingresos, egresos y balance neto para el período. " +
                "Las transacciones anuladas no se incluyen en los totales.")
            .RequireAuthorization()
            .Produces(200)
            .ProducesProblem(401);

        // ── Anular transacción ───────────────────────────────────────────────
        app.MapPatch("/api/v1/cash/transactions/{id:guid}/void",
            async (Guid id, VoidCashRequest body, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new VoidTransactionCommand(id, body.Reason), ct);
                return Results.NoContent();
            })
            .WithTags("Cash")
            .WithSummary("Anular transacción de caja")
            .WithDescription("La transacción queda en el historial como anulada. No se puede reactivar.")
            .RequireAuthorization()
            .Produces(204)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(422)
            .ProducesProblem(401);

        return app;
    }
}

// ── Request bodies ─────────────────────────────────────────────────────────────

/// <summary>Cuerpo del request para registrar una transacción de caja.</summary>
public sealed record RegisterCashRequest(
    DateOnly            Date,                   // Fecha del cobro o pago.
    CashTransactionType Type,                   // 0=Ingreso, 1=Egreso.
    decimal             Amount,                 // Monto positivo en CLP.
    CashPaymentMethod   PaymentMethod,          // 0=Efectivo, 1=Tarjeta, 2=Transferencia.
    TransactionConcept  Concept,                // Ver enum TransactionConcept.
    string?             Description,            // Obligatoria en egresos; libre en ingresos.
    Guid?               MemberId,               // Solo para ingresos vinculados a un socio.
    Guid?               MembershipAssignmentId  // Para vincular a una membresía específica.
);

/// <summary>Cuerpo del request para anular una transacción.</summary>
public sealed record VoidCashRequest(
    string Reason  // Motivo de la anulación (obligatorio).
);
