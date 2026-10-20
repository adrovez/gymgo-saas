using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.Sales.Commands.AddSaleItem;

public sealed class AddSaleItemCommandHandler : IRequestHandler<AddSaleItemCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public AddSaleItemCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<Guid> Handle(AddSaleItemCommand request, CancellationToken cancellationToken)
    {
        var sale = await _context.Sales
            .Include(s => s.Items)
            .Include(s => s.Discounts)
            .FirstOrDefaultAsync(s => s.Id == request.SaleId, cancellationToken)
            ?? throw new NotFoundException("Sale", request.SaleId);

        // Validar que el plan exista si se proporcionó
        if (request.MembershipPlanId.HasValue)
        {
            var planExists = await _context.MembershipPlans
                .AnyAsync(p => p.Id == request.MembershipPlanId.Value && p.IsActive, cancellationToken);

            if (!planExists)
                throw new NotFoundException("MembershipPlan", request.MembershipPlanId.Value);
        }

        var item = sale.AddItem(
            request.ItemType,
            request.Description,
            request.MembershipPlanId,
            request.UnitPrice,
            request.Quantity);

        await _context.SaveChangesAsync(cancellationToken);

        return item.Id;
    }
}
