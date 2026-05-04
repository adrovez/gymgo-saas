using GymGo.Application.Common.Interfaces;
using GymGo.Application.GymEntries.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.GymEntries.Queries.GetGymEntriesByDate;

public sealed class GetGymEntriesByDateQueryHandler
    : IRequestHandler<GetGymEntriesByDateQuery, IReadOnlyList<GymEntryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeProvider _dateTime;

    public GetGymEntriesByDateQueryHandler(
        IApplicationDbContext context,
        IDateTimeProvider dateTime)
    {
        _context  = context;
        _dateTime = dateTime;
    }

    public async Task<IReadOnlyList<GymEntryDto>> Handle(
        GetGymEntriesByDateQuery request,
        CancellationToken cancellationToken)
    {
        var date = request.Date ?? DateOnly.FromDateTime(_dateTime.UtcNow);

        return await _context.GymEntries
            .Where(e => e.EntryDate == date)
            .OrderByDescending(e => e.EnteredAtUtc)
            .Select(e => new GymEntryDto(
                e.Id,
                e.MemberId,
                e.MemberFullName,
                e.MembershipAssignmentId,
                e.EntryDate,
                e.EnteredAtUtc,
                e.ExitedAtUtc,
                e.Method.ToString(),
                e.Notes,
                e.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }
}
