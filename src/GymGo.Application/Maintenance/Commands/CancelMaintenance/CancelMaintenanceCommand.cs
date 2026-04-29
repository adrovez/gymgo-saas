using MediatR;

namespace GymGo.Application.Maintenance.Commands.CancelMaintenance;

public sealed record CancelMaintenanceCommand(
    Guid    Id,
    string? Reason
) : IRequest;
