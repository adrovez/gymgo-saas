using GymGo.API.Extensions;
using GymGo.Application.Sales.Commands.AddSaleDiscount;
using GymGo.Application.Sales.Commands.AddSaleItem;
using GymGo.Application.Sales.Commands.CheckoutSale;
using GymGo.Application.Sales.Commands.ConfirmSale;
using GymGo.Application.Sales.Commands.CreateSale;
using GymGo.Application.Sales.Commands.DeleteDraftSale;
using GymGo.Application.Sales.Commands.RemoveSaleDiscount;
using GymGo.Application.Sales.Commands.RemoveSaleItem;
using GymGo.Application.Sales.Commands.UpdateSaleItem;
using GymGo.Application.Sales.Commands.VoidSale;
using GymGo.Application.Sales.Queries.GetSaleById;
using GymGo.Application.Sales.Queries.GetSales;
using GymGo.Domain.Cash;
using GymGo.Domain.Sales;
using MediatR;

namespace GymGo.API.Endpoints;

public static class SaleEndpoints
{
    public static IEndpointRouteBuilder MapSaleEndpoints(this IEndpointRouteBuilder app)
    {
        // ── Crear venta ──────────────────────────────────────────────────────
        app.MapPost("/api/v1/sales",
            async (CreateSaleRequest body, ISender sender, CancellationToken ct) =>
            {
                var command = new CreateSaleCommand(
                    body.MemberId,
                    body.Date ?? DateOnly.FromDateTime(DateTime.UtcNow),
                    body.PaymentMethod,
                    body.Notes);

                var id = await sender.Send(command, ct);
                return Results.Created($"/api/v1/sales/{id}", new { id });
            })
            .WithTags("Sales")
            .WithSummary("Crear venta en Draft")
            .RequireAuthorization(AuthPolicies.StaffOrAbove)
            .Produces(201)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(401);

        // ── Checkout atómico (crea + confirma en un solo paso) ───────────────
        app.MapPost("/api/v1/sales/checkout",
            async (CheckoutSaleRequest body, ISender sender, CancellationToken ct) =>
            {
                var command = new CheckoutSaleCommand(
                    body.MemberId,
                    body.Date ?? DateOnly.FromDateTime(DateTime.UtcNow),
                    body.PaymentMethod,
                    body.Notes,
                    body.Items
                        .Select(i => new CheckoutSaleItemInput(
                            i.ItemType, i.Description, i.MembershipPlanId, i.UnitPrice, i.Quantity ?? 1))
                        .ToList(),
                    body.Discount is null
                        ? null
                        : new CheckoutSaleDiscountInput(
                            body.Discount.DiscountType, body.Discount.Description, body.Discount.Value));

                var id = await sender.Send(command, ct);
                return Results.Created($"/api/v1/sales/{id}", new { id });
            })
            .WithTags("Sales")
            .WithSummary("Crear y confirmar una venta en un solo paso (POS): venta, ítems, descuento, membresía y caja")
            .RequireAuthorization(AuthPolicies.StaffOrAbove)
            .Produces(201)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(422)
            .ProducesProblem(401);

        // ── Detalle de una venta ─────────────────────────────────────────────
        app.MapGet("/api/v1/sales/{id:guid}",
            async (Guid id, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new GetSaleByIdQuery(id), ct);
                return Results.Ok(result);
            })
            .WithTags("Sales")
            .WithSummary("Detalle de una venta con ítems y descuentos")
            .RequireAuthorization(AuthPolicies.StaffOrAbove)
            .Produces(200)
            .ProducesProblem(404)
            .ProducesProblem(401);

        // ── Listar ventas ────────────────────────────────────────────────────
        app.MapGet("/api/v1/sales",
            async (Guid? memberId, SaleStatus? status, DateOnly? from, DateOnly? to,
                   ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new GetSalesQuery(memberId, status, from, to), ct);
                return Results.Ok(result);
            })
            .WithTags("Sales")
            .WithSummary("Listar ventas con filtros opcionales (memberId, status, from, to)")
            .RequireAuthorization(AuthPolicies.StaffOrAbove)
            .Produces(200)
            .ProducesProblem(401);

        // ── Eliminar venta en Draft ──────────────────────────────────────────
        app.MapDelete("/api/v1/sales/{id:guid}",
            async (Guid id, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new DeleteDraftSaleCommand(id), ct);
                return Results.NoContent();
            })
            .WithTags("Sales")
            .WithSummary("Eliminar venta en Draft (solo ventas en estado Draft)")
            .RequireAuthorization(AuthPolicies.StaffOrAbove)
            .Produces(204)
            .ProducesProblem(422)
            .ProducesProblem(401);

        // ── Agregar ítem ─────────────────────────────────────────────────────
        app.MapPost("/api/v1/sales/{id:guid}/items",
            async (Guid id, AddItemRequest body, ISender sender, CancellationToken ct) =>
            {
                var command = new AddSaleItemCommand(
                    id,
                    body.ItemType,
                    body.Description,
                    body.MembershipPlanId,
                    body.UnitPrice,
                    body.Quantity ?? 1);

                var itemId = await sender.Send(command, ct);
                return Results.Created($"/api/v1/sales/{id}/items/{itemId}", new { id = itemId });
            })
            .WithTags("Sales")
            .WithSummary("Agregar ítem a la venta (solo Draft)")
            .RequireAuthorization(AuthPolicies.StaffOrAbove)
            .Produces(201)
            .ProducesProblem(400)
            .ProducesProblem(422)
            .ProducesProblem(401);

        // ── Editar ítem ──────────────────────────────────────────────────────
        app.MapPut("/api/v1/sales/{id:guid}/items/{itemId:guid}",
            async (Guid id, Guid itemId, UpdateItemRequest body, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new UpdateSaleItemCommand(
                    id, itemId, body.Description, body.UnitPrice, body.Quantity), ct);
                return Results.NoContent();
            })
            .WithTags("Sales")
            .WithSummary("Editar ítem de la venta (solo Draft)")
            .RequireAuthorization(AuthPolicies.StaffOrAbove)
            .Produces(204)
            .ProducesProblem(400)
            .ProducesProblem(422)
            .ProducesProblem(401);

        // ── Eliminar ítem ────────────────────────────────────────────────────
        app.MapDelete("/api/v1/sales/{id:guid}/items/{itemId:guid}",
            async (Guid id, Guid itemId, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new RemoveSaleItemCommand(id, itemId), ct);
                return Results.NoContent();
            })
            .WithTags("Sales")
            .WithSummary("Eliminar ítem de la venta (solo Draft)")
            .RequireAuthorization(AuthPolicies.StaffOrAbove)
            .Produces(204)
            .ProducesProblem(422)
            .ProducesProblem(401);

        // ── Agregar descuento ────────────────────────────────────────────────
        app.MapPost("/api/v1/sales/{id:guid}/discounts",
            async (Guid id, AddDiscountRequest body, ISender sender, CancellationToken ct) =>
            {
                var command = new AddSaleDiscountCommand(
                    id, body.DiscountType, body.Description, body.Value);

                var discountId = await sender.Send(command, ct);
                return Results.Created($"/api/v1/sales/{id}/discounts/{discountId}", new { id = discountId });
            })
            .WithTags("Sales")
            .WithSummary("Agregar descuento a la venta (solo Draft)")
            .RequireAuthorization(AuthPolicies.StaffOrAbove)
            .Produces(201)
            .ProducesProblem(400)
            .ProducesProblem(422)
            .ProducesProblem(401);

        // ── Eliminar descuento ───────────────────────────────────────────────
        app.MapDelete("/api/v1/sales/{id:guid}/discounts/{discountId:guid}",
            async (Guid id, Guid discountId, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new RemoveSaleDiscountCommand(id, discountId), ct);
                return Results.NoContent();
            })
            .WithTags("Sales")
            .WithSummary("Eliminar descuento de la venta (solo Draft)")
            .RequireAuthorization(AuthPolicies.StaffOrAbove)
            .Produces(204)
            .ProducesProblem(422)
            .ProducesProblem(401);

        // ── Confirmar venta ──────────────────────────────────────────────────
        app.MapPatch("/api/v1/sales/{id:guid}/confirm",
            async (Guid id, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new ConfirmSaleCommand(id), ct);
                return Results.NoContent();
            })
            .WithTags("Sales")
            .WithSummary("Confirmar venta: cobra, crea MembershipAssignment y CashTransaction automáticamente")
            .RequireAuthorization(AuthPolicies.StaffOrAbove)
            .Produces(204)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(422)
            .ProducesProblem(401);

        // ── Anular venta ─────────────────────────────────────────────────────
        app.MapPatch("/api/v1/sales/{id:guid}/void",
            async (Guid id, VoidSaleRequest body, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new VoidSaleCommand(id, body.Reason), ct);
                return Results.NoContent();
            })
            .WithTags("Sales")
            .WithSummary("Anular venta confirmada: anula la CashTransaction y cancela la MembershipAssignment")
            .RequireAuthorization(AuthPolicies.StaffOrAbove)
            .Produces(204)
            .ProducesProblem(422)
            .ProducesProblem(401);

        return app;
    }
}

// ── Request bodies ────────────────────────────────────────────────────────────

public sealed record CreateSaleRequest(
    Guid MemberId,
    DateOnly? Date,
    CashPaymentMethod PaymentMethod,
    string? Notes
);

public sealed record AddItemRequest(
    SaleItemType ItemType,
    string Description,
    Guid? MembershipPlanId,
    decimal UnitPrice,
    int? Quantity
);

public sealed record UpdateItemRequest(
    string Description,
    decimal UnitPrice,
    int Quantity
);

public sealed record AddDiscountRequest(
    SaleDiscountType DiscountType,
    string Description,
    decimal Value
);

public sealed record VoidSaleRequest(string Reason);

public sealed record CheckoutSaleItemRequest(
    SaleItemType ItemType,
    string Description,
    Guid? MembershipPlanId,
    decimal UnitPrice,
    int? Quantity
);

public sealed record CheckoutSaleDiscountRequest(
    SaleDiscountType DiscountType,
    string Description,
    decimal Value
);

public sealed record CheckoutSaleRequest(
    Guid MemberId,
    DateOnly? Date,
    CashPaymentMethod PaymentMethod,
    string? Notes,
    List<CheckoutSaleItemRequest> Items,
    CheckoutSaleDiscountRequest? Discount
);
