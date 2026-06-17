using GymGo.Application.Cash.DTOs;
using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Cash;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.Cash.Queries.GetCashSummary;

public sealed class GetCashSummaryQueryHandler : IRequestHandler<GetCashSummaryQuery, CashSummaryDto>
{
    private readonly IApplicationDbContext _context;

    public GetCashSummaryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CashSummaryDto> Handle(GetCashSummaryQuery request, CancellationToken cancellationToken)
    {
        // Trae todo en una sola query y agrega en memoria (volumen de caja es bajo)
        var transactions = await _context.CashTransactions
            .Where(t => t.Date >= request.From && t.Date <= request.To)
            .ToListAsync(cancellationToken);

        var active  = transactions.Where(t => !t.IsVoided).ToList();
        var voided  = transactions.Where(t =>  t.IsVoided).ToList();

        var income   = active.Where(t => t.Type == CashTransactionType.Ingreso).ToList();
        var expenses = active.Where(t => t.Type == CashTransactionType.Egreso).ToList();

        var totalIncome   = income.Sum(t => t.Amount);
        var totalExpenses = expenses.Sum(t => t.Amount);

        var incomeByMethod   = GroupByMethod(income);
        var expensesByMethod = GroupByMethod(expenses);
        var incomeByConcept  = GroupByConcept(income);
        var expensesByConcept = GroupByConcept(expenses);

        return new CashSummaryDto(
            TotalIncome:          totalIncome,
            TotalExpenses:        totalExpenses,
            NetBalance:           totalIncome - totalExpenses,
            IncomeByPaymentMethod:  incomeByMethod,
            ExpensesByPaymentMethod: expensesByMethod,
            IncomeByConcept:      incomeByConcept,
            ExpensesByConcept:    expensesByConcept,
            TransactionCount:     active.Count,
            VoidedCount:          voided.Count);
    }

    private static IReadOnlyDictionary<string, decimal> GroupByMethod(
        IEnumerable<CashTransaction> transactions) =>
        transactions
            .GroupBy(t => t.PaymentMethod.ToString())
            .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));

    private static IReadOnlyDictionary<string, decimal> GroupByConcept(
        IEnumerable<CashTransaction> transactions) =>
        transactions
            .GroupBy(t => t.Concept.ToString())
            .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));
}
