using GymGo.Application.Common.Interfaces;
using GymGo.Application.Sales.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.Sales.Queries.GetSales;

public sealed class GetSalesQueryHandler : IRequestHandler<GetSalesQuery, IReadOnlyList<SaleSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetSalesQueryHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<IReadOnlyList<SaleSummaryDto>> Handle(GetSalesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Sales
            .Include(s => s.Items)
            .AsQueryable();

        if (request.MemberId.HasValue)
            query = query.Where(s => s.MemberId == request.MemberId.Value);

        if (request.Status.HasValue)
            query = query.Where(s => s.Status == request.Status.Value);

        if (request.From.HasValue)
            query = query.Where(s => s.Date >= request.From.Value);

        if (request.To.HasValue)
            query = query.Where(s => s.Date <= request.To.Value);

        var sales = await query
            .OrderByDescending(s => s.Date)
            .ThenByDescending(s => s.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        if (!sales.Any()) return Array.Empty<SaleSummaryDto>();

        // Cargar datos de socios en batch
        var memberIds = sales.Select(s => s.MemberId).Distinct().ToList();
        var members = await _context.Members
            .Where(m => memberIds.Contains(m.Id))
            .ToDictionaryAsync(m => m.Id, cancellationToken);

        return sales.Select(s =>
        {
            members.TryGetValue(s.MemberId, out var member);
            return new SaleSummaryDto(
                s.Id,
                s.MemberId,
                member is not null ? $"{member.FirstName} {member.LastName}" : string.Empty,
                member?.Rut ?? string.Empty,
                s.Date,
                s.Status,
                s.Status.ToLabel(),
                s.PaymentMethod,
                s.PaymentMethod.ToLabel(),
                s.SubtotalAmount,
                s.DiscountAmount,
                s.TotalAmount,
                s.Items.Count,
                s.CreatedAtUtc
            );
        }).ToList();
    }
}
