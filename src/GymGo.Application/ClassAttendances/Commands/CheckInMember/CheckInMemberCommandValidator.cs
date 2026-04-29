using FluentValidation;

namespace GymGo.Application.ClassAttendances.Commands.CheckInMember;

public sealed class CheckInMemberCommandValidator : AbstractValidator<CheckInMemberCommand>
{
    public CheckInMemberCommandValidator()
    {
        RuleFor(x => x.MemberId)
            .NotEmpty()
            .WithMessage("El socio es obligatorio.");

        RuleFor(x => x.ClassScheduleId)
            .NotEmpty()
            .WithMessage("El horario de la clase es obligatorio.");

        RuleFor(x => x.SessionDate)
            .Must(date => date is null || date.Value <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("La fecha de la sesión no puede ser futura.");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .WithMessage("Las observaciones no pueden superar los 500 caracteres.")
            .When(x => x.Notes is not null);
    }
}
