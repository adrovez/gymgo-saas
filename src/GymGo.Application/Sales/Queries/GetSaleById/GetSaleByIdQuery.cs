using GymGo.Application.Sales.DTOs;
using MediatR;

namespace GymGo.Application.Sales.Queries.GetSaleById;

public sealed record GetSaleByIdQuery(Guid SaleId) : IRequest<SaleDto>;
