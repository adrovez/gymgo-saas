using GymGo.Application.GymEntries.DTOs;
using MediatR;

namespace GymGo.Application.GymEntries.Queries.GetGymEntriesByDate;

/// <summary>
/// Devuelve todos los ingresos del gimnasio para una fecha dada.
/// Si <see cref="Date"/> es null, se usa la fecha actual (UTC).
/// </summary>
public sealed record GetGymEntriesByDateQuery(
    DateOnly? Date = null
) : IRequest<IReadOnlyList<GymEntryDto>>;
