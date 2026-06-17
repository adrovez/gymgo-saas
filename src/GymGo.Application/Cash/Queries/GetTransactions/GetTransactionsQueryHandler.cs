using GymGo.Application.Cash.DTOs;
using GymGo.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.Cash.Queries.GetTransactions;

public sealed class GetTransactionsQueryHandler
    : IRequestHandler<GetTransactionsQuery, IReadOnlyList<CashTransactionDto>>
{
    private readonly IApplicationDbContext _context;

    public GetTransactionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CashTransactionDto>> Handle(
        GetTransactionsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.CashTransactions
            .Where(t => t.Date >= request.From && t.Date <= request.To);

        if (request.Type.HasValue)
            query = query.Where(t => t.Type == request.Type.Value);

        if (request.MemberId.HasValue)
            query = query.Where(t => t.MemberId == request.MemberId.Value);

        // Left join a Members para incluir el nombre sin excluir transacciones sin socio
        var results = await query
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.CreatedAtUtc)
            .GroupJoin(
                _context.Members,
                t => t.MemberId,
                m => (Guid?)m.Id,
                (t, members) => new { Transaction = t, Members = members })
            .SelectMany(
                x => x.Members.DefaultIfEmpty(),
                (x, member) => new CashTransactionDto(
                    x.Transaction.Id,
                    x.Transaction.Date,
                    x.Transaction.Type.ToString(),
                    x.Transaction.Amount,
                    x.Transaction.PaymentMethod.ToString(),
                    x.Transaction.Concept.ToString(),
                    x.Transaction.Description,
                    x.Transaction.MemberId,
                    member != null ? member.FirstName + " " + member.LastName : null,
                    x.Transaction.MembershipAssignmentId,
                    x.Transaction.IsVoided,
                    x.Transaction.VoidedAtUtc,
                    x.Transaction.VoidReason,
                    x.Transaction.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        return results;
    }
}
