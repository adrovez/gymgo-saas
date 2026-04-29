using GymGo.Application.GymClasses.DTOs;
using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.GymClasses.Queries.GetGymClassById;

public sealed class GetGymClassByIdQueryHandler : IRequestHandler<GetGymClassByIdQuery, GymClassDto>
{
    private readonly IApplicationDbContext _context;

    public GetGymClassByIdQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<GymClassDto> Handle(GetGymClassByIdQuery request, CancellationToken cancellationToken)
    {
        var gymClass = await _context.GymClasses
            .Include(c => c.Schedules)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("GymClass", request.Id);

        return gymClass.ToDto();
    }
}
