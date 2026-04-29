using GymGo.Domain.ClassAttendances;

namespace GymGo.Application.ClassAttendances.DTOs;

public static class ClassAttendanceMappings
{
    public static ClassAttendanceDto ToDto(this ClassAttendance a) =>
        new(
            Id:              a.Id,
            MemberId:        a.MemberId,
            MemberFullName:  a.MemberFullName,
            ClassScheduleId: a.ClassScheduleId,
            SessionDate:     a.SessionDate,
            CheckedInAtUtc:  a.CheckedInAtUtc,
            CheckInMethod:   a.CheckInMethod,
            Notes:           a.Notes
        );
}
