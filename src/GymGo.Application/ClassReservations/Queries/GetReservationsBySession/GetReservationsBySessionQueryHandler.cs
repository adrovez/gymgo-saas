using GymGo.Application.ClassReservations.DTOs;
using GymGo.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.ClassReservations.Queries.GetReservationsBySession;

public sealed class GetReservationsBySessionQueryHandler
    : IRequestHandler<GetReservationsBySessionQuery, IReadOnlyList<ClassReservationDto>>
{
    private readonly IApplicationDbContext _context;

    public GetReservationsBySessionQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ClassReservationDto>> Handle(
        GetReservationsBySessionQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.ClassReservations
            .Where(r =>
                r.ClassScheduleId == request.ClassScheduleId &&
                r.SessionDate     == request.SessionDate)
            .OrderBy(r => r.ReservedAtUtc)
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
