using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.Sales.Commands.AddSaleDiscount;

public sealed class AddSaleDiscountCommandHandler : IRequestHandler<AddSaleDiscountCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public AddSaleDiscountCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<Guid> Handle(AddSaleDiscountCommand request, CancellationToken cancellationToken)
    {
        var sale = await _context.Sales
            .Include(s => s.Items)
            .Include(s => s.Discounts)
            .FirstOrDefaultAsync(s => s.Id == request.SaleId, cancellationToken)
            ?? throw new NotFoundException("Sale", request.SaleId);

        if (!sale.Items.Any())
            throw new BusinessRuleViolationException(
                "SALE_NO_ITEMS_FOR_DISCOUNT",
                "Agregue al menos un ítem antes de aplicar un descuento.");

        var discount = sale.AddDiscount(request.DiscountType, request.Description, request.Value);

        await _context.SaveChangesAsync(cancellationToken);

        return discount.Id;
    }
}
