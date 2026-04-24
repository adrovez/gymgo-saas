using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.Members.Commands.UpdateMember;

/// <summary>
/// Handler para <see cref="UpdateMemberCommand"/>.
/// </summary>
public sealed class UpdateMemberCommandHandler : IRequestHandler<UpdateMemberCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateMemberCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateMemberCommand request, CancellationToken cancellationToken)
    {
        var member = await _context.Members
            .FirstOrDefaultAsync(m => m.Id == request.MemberId, cancellationToken)
            ?? throw new NotFoundException("Member", request.MemberId);

        member.Update(
            firstName:              request.FirstName,
            lastName:               request.LastName,
            birthDate:              request.BirthDate,
            gender:                 request.Gender,
            email:                  request.Email,
            phone:                  request.Phone,
            address:                request.Address,
            emergencyContactName:   request.EmergencyContactName,
            emergencyContactPhone:  request.EmergencyContactPhone,
            notes:                  request.Notes);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
