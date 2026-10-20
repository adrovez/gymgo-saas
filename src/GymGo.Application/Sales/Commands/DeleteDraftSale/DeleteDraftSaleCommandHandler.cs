using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Exceptions;
using GymGo.Domain.Sales;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.Sales.Commands.DeleteDraftSale;

public sealed class DeleteDraftSaleCommandHandler : IRequestHandler<DeleteDraftSaleCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeProvider _clock;
    private readonly ICurrentUser _currentUser;

    public DeleteDraftSaleCommandHandler(
        IApplicationDbContext context,
        IDateTimeProvider clock,
        ICurrentUser currentUser)
    {
        _context     = context;
        _clock       = clock;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteDraftSaleCommand request, CancellationToken cancellationToken)
    {
        var sale = await _context.Sales
            .FirstOrDefaultAsync(s => s.Id == request.SaleId, cancellationToken)
            ?? throw new NotFoundException("Sale", request.SaleId);

        if (sale.Status != SaleStatus.Draft)
            throw new BusinessRuleViolationException(
                "SALE_DELETE_NOT_DRAFT",
                "Solo se pueden eliminar ventas en estado Draft. Use la anulación para ventas confirmadas.");

        // Soft delete a través de ISoftDeletable
        sale.IsDeleted     = true;
        sale.DeletedAtUtc  = _clock.UtcNow;
        sale.DeletedBy     = _currentUser.UserId?.ToString();

        await _context.SaveChangesAsync(cancellationToken);
    }
}
