using GymGo.Application.Equipment.DTOs;
using MediatR;

namespace GymGo.Application.Equipment.Queries.GetEquipmentById;

public sealed record GetEquipmentByIdQuery(Guid Id) : IRequest<EquipmentDto>;
