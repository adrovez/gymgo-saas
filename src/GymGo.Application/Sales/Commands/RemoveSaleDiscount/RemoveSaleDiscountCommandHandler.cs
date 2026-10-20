using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.Sales.Commands.RemoveSaleDiscount;

public sealed class RemoveSaleDiscountCommandHandler : IRequestHandler<RemoveSaleDiscountCommand>
{
    private readonly IApplicationDbContext _context;

    public RemoveSaleDiscountCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task Handle(RemoveSaleDiscountCommand request, CancellationToken cancellationToken)
    {
        var sale = await _context.Sales
            .Include(s => s.Items)
            .Include(s => s.Discounts)
            .FirstOrDefaultAsync(s => s.Id == request.SaleId, cancellationToken)
            ?? throw new NotFoundException("Sale", request.SaleId);

        sale.RemoveDiscount(request.DiscountId);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
