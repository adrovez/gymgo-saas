using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.Sales.Commands.UpdateSaleItem;

public sealed class UpdateSaleItemCommandHandler : IRequestHandler<UpdateSaleItemCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateSaleItemCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task Handle(UpdateSaleItemCommand request, CancellationToken cancellationToken)
    {
        var sale = await _context.Sales
            .Include(s => s.Items)
            .Include(s => s.Discounts)
            .FirstOrDefaultAsync(s => s.Id == request.SaleId, cancellationToken)
            ?? throw new NotFoundException("Sale", request.SaleId);

        sale.UpdateItem(request.ItemId, request.Description, request.UnitPrice, request.Quantity);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
