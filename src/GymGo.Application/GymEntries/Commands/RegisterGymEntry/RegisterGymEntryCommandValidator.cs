using FluentValidation;

namespace GymGo.Application.GymEntries.Commands.RegisterGymEntry;

public sealed class RegisterGymEntryCommandValidator : AbstractValidator<RegisterGymEntryCommand>
{
    public RegisterGymEntryCommandValidator()
    {
        RuleFor(x => x.MemberId)
            .NotEmpty()
            .WithMessage("El id del socio es obligatorio.");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .WithMessage("Las observaciones no pueden superar los 500 caracteres.");
    }
}
