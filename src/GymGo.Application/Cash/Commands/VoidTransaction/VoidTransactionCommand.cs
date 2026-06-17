using MediatR;

namespace GymGo.Application.Cash.Commands.VoidTransaction;

public sealed record VoidTransactionCommand(Guid TransactionId, string Reason) : IRequest;
