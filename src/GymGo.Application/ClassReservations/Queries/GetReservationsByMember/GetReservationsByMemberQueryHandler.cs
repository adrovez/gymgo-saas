using GymGo.Application.ClassReservations.DTOs;
using GymGo.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.ClassReservations.Queries.GetReservationsByMember;

public sealed class GetReservationsByMemberQueryHandler
    : IRequestHandler<GetReservationsByMemberQuery, IReadOnlyList<ClassReservationDto>>
{
    private readonly IApplicationDbContext _context;

    public GetReservationsByMemberQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ClassReservationDto>> Handle(
        GetReservationsByMemberQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.ClassReservations
            .Where(r => r.MemberId == request.MemberId);

        if (request.Status.HasValue)
            query = query.Where(r => r.Status == request.Status.Value);

        if (request.From.HasValue)
            query = query.Where(r => r.SessionDate >= request.From.Value);

        if (request.To.HasValue)
            query = query.Where(r => r.SessionDate <= request.To.Value);

        return await query
            .OrderByDescending(r => r.SessionDate)
            .ThenBy(r => r.ReservedAtUtc)
            .Select(r => new ClassReservationDto(
                r.Id,
                r.MemberId,
                r.MemberFullName,
                r.ClassScheduleId,
                r.SessionDate,
                r.ReservedAtUtc,
                r.Status,
                r.Notes,
                r.CancelledAtUtc,
                r.CancelledBy,
                r.CancelReason))
            .ToListAsync(cancellationToken);
    }
}
