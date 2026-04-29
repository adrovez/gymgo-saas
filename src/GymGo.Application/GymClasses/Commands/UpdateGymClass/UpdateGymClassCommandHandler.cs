using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.GymClasses.Commands.UpdateGymClass;

public sealed class UpdateGymClassCommandHandler : IRequestHandler<UpdateGymClassCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateGymClassCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(UpdateGymClassCommand request, CancellationToken cancellationToken)
    {
        var gymClass = await _context.GymClasses
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("GymClass", request.Id);

        gymClass.Update(
            name:            request.Name,
            description:     request.Description,
            category:        request.Category,
            color:           request.Color,
            durationMinutes: request.DurationMinutes,
            maxCapacity:     request.MaxCapacity);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
