using GymGo.Application.WorkoutLogs.DTOs;
using MediatR;

namespace GymGo.Application.WorkoutLogs.Queries.GetWorkoutLogs;

public sealed record GetWorkoutLogsQuery(
    Guid MemberId,
    Guid? WorkoutPlanId,
    DateOnly? From,
    DateOnly? To
) : IRequest<IReadOnlyList<WorkoutLogSummaryDto>>;
