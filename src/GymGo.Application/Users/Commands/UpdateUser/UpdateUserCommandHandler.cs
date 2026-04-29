using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.Users.Commands.UpdateUser;

/// <summary>
/// Handler para <see cref="UpdateUserCommand"/>.
/// </summary>
public sealed class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateUserCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user is null)
            throw new NotFoundException(nameof(user), request.UserId);

        user.Update(request.FullName, request.Role);

        if (request.IsActive)
            user.Activate();
        else
            user.Deactivate();

        await _context.SaveChangesAsync(cancellationToken);
    }
}
