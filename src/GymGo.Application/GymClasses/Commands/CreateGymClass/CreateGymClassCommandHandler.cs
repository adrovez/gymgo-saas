using GymGo.Application.Common.Interfaces;
using GymGo.Domain.GymClasses;
using MediatR;

namespace GymGo.Application.GymClasses.Commands.CreateGymClass;

public sealed class CreateGymClassCommandHandler : IRequestHandler<CreateGymClassCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenant _currentTenant;

    public CreateGymClassCommandHandler(IApplicationDbContext context, ICurrentTenant currentTenant)
    {
        _context       = context;
        _currentTenant = currentTenant;
    }

    public async Task<Guid> Handle(CreateGymClassCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _currentTenant.TenantId
            ?? throw new UnauthorizedAccessException("No se pudo determinar el tenant actual.");

        var gymClass = GymClass.Create(
            tenantId:        tenantId,
            name:            request.Name,
            description:     request.Description,
            category:        request.Category,
            color:           request.Color,
            durationMinutes: request.DurationMinutes,
            maxCapacity:     request.MaxCapacity);

        _context.GymClasses.Add(gymClass);
        await _context.SaveChangesAsync(cancellationToken);

        return gymClass.Id;
    }
}
