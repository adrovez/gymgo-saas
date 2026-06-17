using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.Cash.Commands.VoidTransaction;

public sealed class VoidTransactionCommandHandler : IRequestHandler<VoidTransactionCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeProvider _clock;

    public VoidTransactionCommandHandler(IApplicationDbContext context, IDateTimeProvider clock)
    {
        _context = context;
        _clock   = clock;
    }

    public async Task Handle(VoidTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = await _context.CashTransactions
            .FirstOrDefaultAsync(t => t.Id == request.TransactionId, cancellationToken)
            ?? throw new NotFoundException("CashTransaction", request.TransactionId);

        transaction.Void(_clock.UtcNow, request.Reason);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
