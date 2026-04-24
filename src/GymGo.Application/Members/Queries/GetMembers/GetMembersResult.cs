using GymGo.Application.Members.DTOs;

namespace GymGo.Application.Members.Queries.GetMembers;

/// <summary>
/// Resultado paginado de la consulta de socios.
/// </summary>
public sealed record GetMembersResult(
    IReadOnlyList<MemberSummaryDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);
