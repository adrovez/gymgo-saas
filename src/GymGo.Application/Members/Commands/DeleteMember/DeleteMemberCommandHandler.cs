using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.Members.Commands.DeleteMember;

/// <summary>
/// Handler para <see cref="DeleteMemberCommand"/>.
/// El interceptor de auditoría convierte el Delete en soft-delete automáticamente.
/// </summary>
public sealed class DeleteMemberCommandHandler : IRequestHandler<DeleteMemberCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteMemberCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteMemberCommand request, CancellationToken cancellationToken)
    {
        var member = await _context.Members
            .FirstOrDefaultAsync(m => m.Id == request.MemberId, cancellationToken)
            ?? throw new NotFoundException("Member", request.MemberId);

        _context.Members.Remove(member);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
