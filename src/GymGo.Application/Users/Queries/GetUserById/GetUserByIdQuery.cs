using GymGo.Application.Users.DTOs;
using MediatR;

namespace GymGo.Application.Users.Queries.GetUserById;

/// <summary>
/// Consulta para obtener un usuario por su Id.
/// </summary>
public sealed record GetUserByIdQuery(Guid UserId) : IRequest<UserDto>;
