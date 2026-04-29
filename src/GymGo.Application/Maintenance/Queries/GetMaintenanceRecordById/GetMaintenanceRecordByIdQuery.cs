using GymGo.Application.Maintenance.DTOs;
using MediatR;

namespace GymGo.Application.Maintenance.Queries.GetMaintenanceRecordById;

public sealed record GetMaintenanceRecordByIdQuery(Guid Id) : IRequest<MaintenanceRecordDto>;
