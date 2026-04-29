using FluentValidation;

namespace GymGo.Application.ClassReservations.Commands.CreateReservation;

public sealed class CreateReservationCommandValidator : AbstractValidator<CreateReservationCommand>
{
    public CreateReservationCommandValidator()
    {
        RuleFor(x => x.MemberId)
            .NotEmpty()
            .WithMessage("El id del socio es obligatorio.");

        RuleFor(x => x.ClassScheduleId)
            .NotEmpty()
            .WithMessage("El id del horario es obligatorio.");

        RuleFor(x => x.SessionDate)
            .NotEmpty()
            .WithMessage("La fecha de la sesión es obligatoria.");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .WithMessage("Las observaciones no pueden superar los 500 caracteres.");
    }
}
