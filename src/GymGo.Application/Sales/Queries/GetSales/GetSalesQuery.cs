using GymGo.Application.Sales.DTOs;
using GymGo.Domain.Sales;
using MediatR;

namespace GymGo.Application.Sales.Queries.GetSales;

/// <summary>
/// Lista ventas con filtros opcionales.
/// </summary>
public sealed record GetSalesQuery(
    Guid? MemberId,
    SaleStatus? Status,
    DateOnly? From,
    DateOnly? To
) : IRequest<IReadOnlyList<SaleSummaryDto>>;
