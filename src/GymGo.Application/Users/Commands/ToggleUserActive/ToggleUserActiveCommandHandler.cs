using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.Users.Commands.ToggleUserActive;

/// <summary>
/// Handler para <see cref="ToggleUserActiveCommand"/>.
/// </summary>
public sealed class ToggleUserActiveCommandHandler : IRequestHandler<ToggleUserActiveCommand>
{
    private readonly IApplicationDbContext _context;

    public ToggleUserActiveCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ToggleUserActiveCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user is null)
            throw new NotFoundException(nameof(user), request.UserId);

        if (request.IsActive)
            user.Activate();
        else
            user.Deactivate();

        await _context.SaveChangesAsync(cancellationToken);
    }
}
