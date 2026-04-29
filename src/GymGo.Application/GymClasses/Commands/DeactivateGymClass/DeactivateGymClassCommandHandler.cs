using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.GymClasses.Commands.DeactivateGymClass;

public sealed class DeactivateGymClassCommandHandler : IRequestHandler<DeactivateGymClassCommand>
{
    private readonly IApplicationDbContext _context;

    public DeactivateGymClassCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(DeactivateGymClassCommand request, CancellationToken cancellationToken)
    {
        var gymClass = await _context.GymClasses
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("GymClass", request.Id);

        gymClass.Deactivate();
        await _context.SaveChangesAsync(cancellationToken);
    }
}
