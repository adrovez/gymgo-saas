using MediatR;

namespace GymGo.Application.Maintenance.Commands.StartMaintenance;

public sealed record StartMaintenanceCommand(Guid Id) : IRequest;
