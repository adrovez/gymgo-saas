using FluentValidation;

namespace GymGo.Application.GymEntries.Commands.RegisterGymExit;

public sealed class RegisterGymExitCommandValidator : AbstractValidator<RegisterGymExitCommand>
{
    public RegisterGymExitCommandValidator()
    {
        RuleFor(x => x.EntryId)
            .NotEmpty()
            .WithMessage("El id del registro de ingreso es obligatorio.");
    }
}
