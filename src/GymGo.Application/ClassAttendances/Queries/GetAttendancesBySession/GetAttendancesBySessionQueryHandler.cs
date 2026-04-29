using GymGo.Application.ClassAttendances.DTOs;
using GymGo.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.ClassAttendances.Queries.GetAttendancesBySession;

public sealed class GetAttendancesBySessionQueryHandler
    : IRequestHandler<GetAttendancesBySessionQuery, IReadOnlyList<ClassAttendanceDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeProvider _dateTime;

    public GetAttendancesBySessionQueryHandler(
        IApplicationDbContext context,
        IDateTimeProvider dateTime)
    {
        _context  = context;
        _dateTime = dateTime;
    }

    public async Task<IReadOnlyList<ClassAttendanceDto>> Handle(
        GetAttendancesBySessionQuery request,
        CancellationToken cancellationToken)
    {
        var sessionDate = request.SessionDate
            ?? DateOnly.FromDateTime(_dateTime.UtcNow);

        var attendances = await _context.ClassAttendances
            .Where(a =>
                a.ClassScheduleId == request.ClassScheduleId &&
                a.SessionDate     == sessionDate)
            .OrderBy(a => a.CheckedInAtUtc)
            .Select(a => a.ToDto())
            .ToListAsync(cancellationToken);

        return attendances.AsReadOnly();
    }
}
