using GymGo.Application.Common.Interfaces;
using GymGo.Application.Members.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.Members.Queries.GetMembers;

/// <summary>
/// Handler para <see cref="GetMembersQuery"/>.
/// El query filter de EF Core garantiza que sólo se devuelven socios del tenant actual
/// y que los soft-deleted quedan excluidos automáticamente.
/// </summary>
public sealed class GetMembersQueryHandler : IRequestHandler<GetMembersQuery, GetMembersResult>
{
    private readonly IApplicationDbContext _context;

    public GetMembersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GetMembersResult> Handle(GetMembersQuery request, CancellationToken cancellationToken)
    {
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var page = Math.Max(request.Page, 1);

        var query = _context.Members.AsNoTracking();

        // Filtro por estado
        if (request.Status.HasValue)
            query = query.Where(m => m.Status == request.Status.Value);

        // Búsqueda por nombre, apellido o RUT
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(m =>
                m.FirstName.ToLower().Contains(search) ||
                m.LastName.ToLower().Contains(search)  ||
                m.Rut.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var entities = await query
            .OrderBy(m => m.LastName)
            .ThenBy(m => m.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = entities.Select(m => m.ToSummaryDto()).ToList();

        return new GetMembersResult(items, totalCount, page, pageSize, totalPages);
    }
}
