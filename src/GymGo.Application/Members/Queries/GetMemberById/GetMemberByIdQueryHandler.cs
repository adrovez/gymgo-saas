using GymGo.Application.Common.Interfaces;
using GymGo.Application.Members.DTOs;
using GymGo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.Members.Queries.GetMemberById;

/// <summary>
/// Handler para <see cref="GetMemberByIdQuery"/>.
/// </summary>
public sealed class GetMemberByIdQueryHandler : IRequestHandler<GetMemberByIdQuery, MemberDto>
{
    private readonly IApplicationDbContext _context;

    public GetMemberByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MemberDto> Handle(GetMemberByIdQuery request, CancellationToken cancellationToken)
    {
        var member = await _context.Members
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == request.MemberId, cancellationToken)
            ?? throw new NotFoundException("Member", request.MemberId);

        return member.ToDto();
    }
}
