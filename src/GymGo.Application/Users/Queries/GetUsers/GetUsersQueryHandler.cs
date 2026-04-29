using GymGo.Application.Common.Interfaces;
using GymGo.Application.Users.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.Users.Queries.GetUsers;

/// <summary>
/// Handler para <see cref="GetUsersQuery"/>.
/// El HasQueryFilter de EF Core filtra automáticamente por tenant actual y excluye soft-deleted.
/// </summary>
public sealed class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, GetUsersResult>
{
    private readonly IApplicationDbContext _context;

    public GetUsersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GetUsersResult> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Users.AsNoTracking();

        // Filtro por rol
        if (request.Role.HasValue)
            query = query.Where(u => u.Role == request.Role.Value);

        // Búsqueda por nombre o email
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(u =>
                u.FullName.ToLower().Contains(search) ||
                u.Email.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var entities = await query
            .OrderBy(u => u.FullName)
            .ToListAsync(cancellationToken);

        var items = entities.Select(u => u.ToSummaryDto()).ToList();

        return new GetUsersResult(items, totalCount);
    }
}
