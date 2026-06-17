using GymGo.Application.Cash.DTOs;
using MediatR;

namespace GymGo.Application.Cash.Queries.GetCashSummary;

/// <summary>
/// Retorna el resumen de caja para un período.
/// Solo incluye transacciones no anuladas en los totales.
/// </summary>
public sealed record GetCashSummaryQuery(DateOnly From, DateOnly To) : IRequest<CashSummaryDto>;
