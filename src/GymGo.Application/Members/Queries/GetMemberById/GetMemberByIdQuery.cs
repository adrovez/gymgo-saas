using GymGo.Application.Members.DTOs;
using MediatR;

namespace GymGo.Application.Members.Queries.GetMemberById;

/// <summary>
/// Consulta para obtener el detalle completo de un socio por su Id.
/// </summary>
public sealed record GetMemberByIdQuery(Guid MemberId) : IRequest<MemberDto>;
