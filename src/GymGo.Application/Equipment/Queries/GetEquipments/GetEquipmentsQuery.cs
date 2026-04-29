using GymGo.Application.Equipment.DTOs;
using MediatR;

namespace GymGo.Application.Equipment.Queries.GetEquipments;

/// <summary>
/// Devuelve la lista de máquinas del tenant.
/// Si <paramref name="IsActive"/> es null, devuelve todas.
/// </summary>
public sealed record GetEquipmentsQuery(bool? IsActive) : IRequest<IReadOnlyList<EquipmentSummaryDto>>;
