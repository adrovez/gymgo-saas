using GymGo.Application.WorkoutPlans.DTOs;
using GymGo.Domain.WorkoutLogs;
using MediatR;

namespace GymGo.Application.WorkoutPlans.Queries.GetWorkoutPlans;

public sealed record GetWorkoutPlansQuery(
    Guid MemberId,
    WorkoutPlanStatus? Status
) : IRequest<IReadOnlyList<WorkoutPlanSummaryDto>>;
