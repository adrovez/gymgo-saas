using GymGo.Application.Common.Interfaces;
using GymGo.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymGo.Application.GymEntries.Commands.RegisterGymExit;

public sealed class RegisterGymExitCommandHandler : IRequestHandler<RegisterGymExitCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeProvider _dateTime;

    public RegisterGymExitCommandHandler(
        IApplicationDbContext context,
        IDateTimeProvider dateTime)
    {
        _context  = context;
        _dateTime = dateTime;
    }

    public async Task Handle(RegisterGymExitCommand request, CancellationToken cancellationToken)
    {
        // ── 1. Buscar el registro de ingreso ──────────────────────────────────
        var entry = await _context.GymEntries
            .FirstOrDefaultAsync(e => e.Id == request.EntryId, cancellationToken)
            ?? throw new NotFoundException("GymEntry", request.EntryId);

        // ── 2. Registrar la salida (las validaciones están en el dominio) ─────
        entry.RegisterExit(_dateTime.UtcNow);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
