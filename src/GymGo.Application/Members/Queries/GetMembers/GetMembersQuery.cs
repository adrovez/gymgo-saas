using GymGo.Application.Members.DTOs;
using GymGo.Domain.Members;
using MediatR;

namespace GymGo.Application.Members.Queries.GetMembers;

/// <summary>
/// Consulta para listar socios del tenant actual.
/// Soporta filtros opcionales de búsqueda y paginación simple.
/// </summary>
public sealed record GetMembersQuery(
    /// <summary>Filtra por nombre, apellido o RUT (búsqueda parcial, case-insensitive).</summary>
    string? Search = null,

    /// <summary>Filtra por estado del socio. Si es null, trae todos.</summary>
    MemberStatus? Status = null,

    /// <summary>Número de página (base 1).</summary>
    int Page = 1,

    /// <summary>Cantidad de registros por página (máximo 100).</summary>
    int PageSize = 20
) : IRequest<GetMembersResult>;
