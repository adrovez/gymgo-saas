using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.GymClasses.Commands.ReactivateGymClass;

public sealed class ReactivateGymClassCommandHandler : IRequestHandler<ReactivateGymClassCommand>
{
    private readonly IApplicationDbContext _context;

    public ReactivateGymClassCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(ReactivateGymClassCommand request, CancellationToken cancellationToken)
    {
        var gymClass = await _context.GymClasses
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("GymClass", request.Id);

        gymClass.Reactivate();
        await _context.SaveChangesAsync(cancellationToken);
    }
}
