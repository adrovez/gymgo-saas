using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Exceptions;
using GymGo.Domain.Members;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.MembershipAssignments.Commands.RegisterPayment;

/// <summary>
/// Registra el pago y, si el socio estaba Delinquent, lo reactiva a Active.
/// Ambos cambios se persisten en una sola transacción.
/// </summary>
public sealed class RegisterPaymentCommandHandler : IRequestHandler<RegisterPaymentCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeProvider _clock;

    public RegisterPaymentCommandHandler(IApplicationDbContext context, IDateTimeProvider clock)
    {
        _context = context;
        _clock = clock;
    }

    public async Task Handle(RegisterPaymentCommand request, CancellationToken cancellationToken)
    {
        var assignment = await _context.MembershipAssignments
            .FirstOrDefaultAsync(a => a.Id == request.AssignmentId, cancellationToken)
            ?? throw new NotFoundException("MembershipAssignment", request.AssignmentId);

        assignment.RegisterPayment(_clock.UtcNow);

        // Si el socio estaba marcado como moroso, reactivarlo
        var member = await _context.Members
            .FirstOrDefaultAsync(m => m.Id == assignment.MemberId, cancellationToken);

        if (member?.Status == MemberStatus.Delinquent)
            member.Activate();

        await _context.SaveChangesAsync(cancellationToken);
    }
}
