using MediatR;

namespace GymGo.Application.Maintenance.Commands.CompleteMaintenance;

public sealed record CompleteMaintenanceCommand(
    Guid     Id,
    string?  Notes,
    decimal? Cost
) : IRequest;
