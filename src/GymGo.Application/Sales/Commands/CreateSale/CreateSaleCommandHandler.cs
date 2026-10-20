using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Exceptions;
using GymGo.Domain.Sales;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.Sales.Commands.CreateSale;

public sealed class CreateSaleCommandHandler : IRequestHandler<CreateSaleCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTimeProvider _clock;

    public CreateSaleCommandHandler(
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

    public async Task<Guid> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _currentTenant.TenantId
            ?? throw new UnauthorizedAccessException("No se pudo determinar el tenant actual.");

        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("No se pudo determinar el usuario actual.");

        var memberExists = await _context.Members
            .AnyAsync(m => m.Id == request.MemberId, cancellationToken);

        if (!memberExists)
            throw new NotFoundException("Member", request.MemberId);

        var sale = Sale.Create(
            tenantId:          tenantId,
            memberId:          request.MemberId,
            date:              request.Date,
            paymentMethod:     request.PaymentMethod,
            notes:             request.Notes,
            processedByUserId: userId);

        _context.Sales.Add(sale);
        await _context.SaveChangesAsync(cancellationToken);

        return sale.Id;
    }
}
