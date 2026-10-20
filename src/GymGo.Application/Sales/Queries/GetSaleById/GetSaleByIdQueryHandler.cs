using GymGo.Application.Common.Interfaces;
using GymGo.Application.Sales.DTOs;
using GymGo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.Sales.Queries.GetSaleById;

public sealed class GetSaleByIdQueryHandler : IRequestHandler<GetSaleByIdQuery, SaleDto>
{
    private readonly IApplicationDbContext _context;

    public GetSaleByIdQueryHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<SaleDto> Handle(GetSaleByIdQuery request, CancellationToken cancellationToken)
    {
        var sale = await _context.Sales
            .Include(s => s.Items)
            .Include(s => s.Discounts)
            .FirstOrDefaultAsync(s => s.Id == request.SaleId, cancellationToken)
            ?? throw new NotFoundException("Sale", request.SaleId);

        var member = await _context.Members
            .FirstOrDefaultAsync(m => m.Id == sale.MemberId, cancellationToken);

        // Cargar nombres de planes para ítems de tipo Plan
        var planIds = sale.Items
            .Where(i => i.MembershipPlanId.HasValue)
            .Select(i => i.MembershipPlanId!.Value)
            .Distinct()
            .ToList();

        var planNames = planIds.Count > 0
            ? await _context.MembershipPlans
                .Where(p => planIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.Name, cancellationToken)
            : new Dictionary<Guid, string>();

        var items = sale.Items
            .Select(i => i.ToDto(i.MembershipPlanId.HasValue && planNames.TryGetValue(i.MembershipPlanId.Value, out var n) ? n : null))
            .ToList();

        var discounts = sale.Discounts
            .Select(d => d.ToDto())
            .ToList();

        return new SaleDto(
            sale.Id,
            sale.TenantId,
            sale.MemberId,
            member is not null ? $"{member.FirstName} {member.LastName}" : string.Empty,
            member?.Rut ?? string.Empty,
            sale.Date,
            sale.Status,
            sale.Status.ToLabel(),
            sale.PaymentMethod,
            sale.PaymentMethod.ToLabel(),
            sale.SubtotalAmount,
            sale.DiscountAmount,
            sale.TotalAmount,
            sale.MembershipAssignmentId,
            sale.CashTransactionId,
            sale.Notes,
            sale.ProcessedByUserId,
            sale.VoidedAtUtc,
            sale.VoidReason,
            sale.CreatedAtUtc,
            sale.CreatedBy,
            sale.ModifiedAtUtc,
            sale.ModifiedBy,
            items,
            discounts
        );
    }
}
