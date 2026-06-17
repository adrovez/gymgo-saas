using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Cash;
using GymGo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.Cash.Commands.RegisterTransaction;

public sealed class RegisterTransactionCommandHandler : IRequestHandler<RegisterTransactionCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTimeProvider _clock;

    public RegisterTransactionCommandHandler(
        IApplicationDbContext context,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser,
        IDateTimeProvider clock)
    {
        _context       = context;
        _currentTenant = currentTenant;
        _currentUser   = currentUser;
        _clock         = clock;
    }

    public async Task<Guid> Handle(RegisterTransactionCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _currentTenant.TenantId
            ?? throw new UnauthorizedAccessException("No se pudo determinar el tenant actual.");

        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("No se pudo determinar el usuario actual.");

        // Verificar que el socio existe si se informó
        if (request.MemberId.HasValue)
        {
            var memberExists = await _context.Members
                .AnyAsync(m => m.Id == request.MemberId.Value, cancellationToken);

            if (!memberExists)
                throw new NotFoundException("Member", request.MemberId.Value);
        }

        var transaction = CashTransaction.Create(
            tenantId:               tenantId,
            date:                   request.Date,
            type:                   request.Type,
            amount:                 request.Amount,
            paymentMethod:          request.PaymentMethod,
            concept:                request.Concept,
            description:            request.Description,
            memberId:               request.MemberId,
            membershipAssignmentId: request.MembershipAssignmentId,
            processedByUserId:      userId,
            createdAtUtc:           _clock.UtcNow);

        _context.CashTransactions.Add(transaction);
        await _context.SaveChangesAsync(cancellationToken);

        return transaction.Id;
    }
}
