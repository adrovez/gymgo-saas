using GymGo.Application.Users.DTOs;

namespace GymGo.Application.Users.Queries.GetUsers;

/// <summary>
/// Resultado de <see cref="GetUsersQuery"/>.
/// </summary>
public sealed record GetUsersResult(
    IReadOnlyList<UserSummaryDto> Items,
    int TotalCount
);
