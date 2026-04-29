using GymGo.Application.GymClasses.DTOs;
using MediatR;

namespace GymGo.Application.GymClasses.Queries.GetGymClasses;

public sealed record GetGymClassesQuery(bool? IsActive = null) : IRequest<IReadOnlyList<GymClassSummaryDto>>;
