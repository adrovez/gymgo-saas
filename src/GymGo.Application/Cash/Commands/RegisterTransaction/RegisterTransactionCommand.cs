using GymGo.Domain.Cash;
using MediatR;

namespace GymGo.Application.Cash.Commands.RegisterTransaction;

public sealed record RegisterTransactionCommand(
    DateOnly             Date,
    CashTransactionType  Type,
    decimal              Amount,
    CashPaymentMethod    PaymentMethod,
    TransactionConcept   Concept,
    string?              Description,
    Guid?                MemberId,
    Guid?                MembershipAssignmentId
) : IRequest<Guid>;
