using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.Sales.Commands.RemoveSaleItem;

public sealed class RemoveSaleItemCommandHandler : IRequestHandler<RemoveSaleItemCommand>
{
    private readonly IApplicationDbContext _context;

    public RemoveSaleItemCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task Handle(RemoveSaleItemCommand request, CancellationToken cancellationToken)
    {
        var sale = await _context.Sales
            .Include(s => s.Items)
            .Include(s => s.Discounts)
            .FirstOrDefaultAsync(s => s.Id == request.SaleId, cancellationToken)
            ?? throw new NotFoundException("Sale", request.SaleId);

        sale.RemoveItem(request.ItemId);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
