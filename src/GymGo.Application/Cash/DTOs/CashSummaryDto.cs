namespace GymGo.Application.Cash.DTOs;

public sealed record CashSummaryDto(
    decimal TotalIncome,
    decimal TotalExpenses,
    decimal NetBalance,
    IReadOnlyDictionary<string, decimal> IncomeByPaymentMethod,
    IReadOnlyDictionary<string, decimal> ExpensesByPaymentMethod,
    IReadOnlyDictionary<string, decimal> IncomeByConcept,
    IReadOnlyDictionary<string, decimal> ExpensesByConcept,
    int TransactionCount,
    int VoidedCount
);
