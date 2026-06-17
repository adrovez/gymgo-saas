namespace GymGo.Application.MembershipAssignments.DTOs;

/// <summary>
/// Respuesta del endpoint GET /api/v1/assignments/expiring.
/// Agrupa membresías por vencer (próximos 7 días) y vencidas recientemente (últimos 14 días).
/// Filtra por EndDate, no por Status, porque el status no se actualiza automáticamente.
/// </summary>
public sealed record ExpiringAssignmentsDto(
    IReadOnlyList<MembershipAssignmentSummaryDto> ExpiringSoon,
    IReadOnlyList<MembershipAssignmentSummaryDto> RecentlyExpired
);
