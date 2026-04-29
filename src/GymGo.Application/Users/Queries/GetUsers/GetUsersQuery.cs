using GymGo.Domain.Users;
using MediatR;

namespace GymGo.Application.Users.Queries.GetUsers;

/// <summary>
/// Consulta para listar usuarios del tenant actual.
/// Soporta filtros opcionales de búsqueda por nombre/email y por rol.
/// </summary>
public sealed record GetUsersQuery(
    /// <summary>Filtra por nombre o email (búsqueda parcial, case-insensitive).</summary>
    string? Search = null,

    /// <summary>Filtra por rol. Si es null, trae todos.</summary>
    UserRole? Role = null
) : IRequest<GetUsersResult>;
