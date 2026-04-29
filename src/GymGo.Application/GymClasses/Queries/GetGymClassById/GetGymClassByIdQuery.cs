using GymGo.Application.GymClasses.DTOs;
using MediatR;

namespace GymGo.Application.GymClasses.Queries.GetGymClassById;

public sealed record GetGymClassByIdQuery(Guid Id) : IRequest<GymClassDto>;
