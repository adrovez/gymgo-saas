using MediatR;

namespace GymGo.Application.Members.Commands.DeleteMember;

/// <summary>
/// Comando para dar de baja (soft delete) a un socio.
/// El registro se conserva en la base de datos con IsDeleted = true.
/// </summary>
public sealed record DeleteMemberCommand(Guid MemberId) : IRequest;
