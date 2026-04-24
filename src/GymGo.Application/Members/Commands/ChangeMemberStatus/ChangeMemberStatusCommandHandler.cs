using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Exceptions;
using GymGo.Domain.Members;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.Members.Commands.ChangeMemberStatus;

/// <summary>
/// Handler para <see cref="ChangeMemberStatusCommand"/>.
/// </summary>
public sealed class ChangeMemberStatusCommandHandler : IRequestHandler<ChangeMemberStatusCommand>
{
    private readonly IApplicationDbContext _context;

    public ChangeMemberStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ChangeMemberStatusCommand request, CancellationToken cancellationToken)
    {
        var member = await _context.Members
            .FirstOrDefaultAsync(m => m.Id == request.MemberId, cancellationToken)
            ?? throw new NotFoundException("Member", request.MemberId);

        switch (request.NewStatus)
        {
            case MemberStatus.Active:
                member.Activate();
                break;
            case MemberStatus.Suspended:
                member.Suspend();
                break;
            case MemberStatus.Delinquent:
                member.MarkAsDelinquent();
                break;
            default:
                throw new BusinessRuleViolationException(
                    "MEMBER_STATUS_INVALID",
                    $"El estado '{request.NewStatus}' no es válido.");
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
