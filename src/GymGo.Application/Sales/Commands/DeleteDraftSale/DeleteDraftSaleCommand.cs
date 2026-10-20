using MediatR;

namespace GymGo.Application.Sales.Commands.DeleteDraftSale;

/// <summary>
/// Elimina (soft delete) una venta en estado Draft.
/// Solo permitido mientras la venta no haya sido confirmada.
/// </summary>
public sealed record DeleteDraftSaleCommand(Guid SaleId) : IRequest;
