using GymGo.Application.Cash.DTOs;
using GymGo.Domain.Cash;
using MediatR;

namespace GymGo.Application.Cash.Queries.GetTransactions;

/// <summary>
/// Retorna las transacciones de caja del tenant filtradas por fecha y tipo.
/// Siempre incluye las anuladas (IsVoided) para mantener el historial completo.
/// </summary>
public sealed record GetTransactionsQuery(
    DateOnly           From,
    DateOnly           To,
    CashTransactionType? Type    = null,
    Guid?              MemberId = null
) : IRequest<IReadOnlyList<CashTransactionDto>>;
