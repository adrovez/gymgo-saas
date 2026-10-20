using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.Sales.Commands.VoidSale;

public sealed class VoidSaleCommandHandler : IRequestHandler<VoidSaleCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeProvider _clock;

    public VoidSaleCommandHandler(IApplicationDbContext context, IDateTimeProvider clock)
    {
        _context = context;
        _clock   = clock;
    }

    public async Task Handle(VoidSaleCommand request, CancellationToken cancellationToken)
    {
        var sale = await _context.Sales
            .Include(s => s.Items)
            .Include(s => s.Discounts)
            .FirstOrDefaultAsync(s => s.Id == request.SaleId, cancellationToken)
            ?? throw new NotFoundException("Sale", request.SaleId);

        var utcNow = _clock.UtcNow;

        // Anular TODAS las CashTransactions generadas por esta venta (puede haber
        // más de una: el checkout crea un movimiento por cada concepto presente).
        var cashTransactions = await _context.CashTransactions
            .Where(t => t.SaleId == sale.Id && !t.IsVoided)
            .ToListAsync(cancellationToken);

        foreach (var cashTx in cashTransactions)
            cashTx.Void(utcNow, $"Venta anulada: {request.Reason}");

        // Cancelar la MembershipAssignment
        if (sale.MembershipAssignmentId.HasValue)
        {
            var assignment = await _context.MembershipAssignments
                .FirstOrDefaultAsync(a => a.Id == sale.MembershipAssignmentId.Value, cancellationToken);

            assignment?.Cancel();
        }

        // Anular la venta
        sale.Void(utcNow, request.Reason);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
