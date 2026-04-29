using GymGo.Domain.GymEntries;

namespace GymGo.Application.GymEntries.DTOs;

public static class GymEntryMappings
{
    public static GymEntryDto ToDto(this GymEntry entry) => new(
        Id:                    entry.Id,
        MemberId:              entry.MemberId,
        MemberFullName:        entry.MemberFullName,
        MembershipAssignmentId: entry.MembershipAssignmentId,
        EntryDate:             entry.EntryDate,
        EnteredAtUtc:          entry.EnteredAtUtc,
        Method:                entry.Method.ToString(),
        Notes:                 entry.Notes,
        CreatedAtUtc:          entry.CreatedAtUtc
    );
}
