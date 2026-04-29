using FluentValidation;

namespace GymGo.Application.GymClasses.Commands.UpdateGymClass;

public sealed class UpdateGymClassCommandValidator : AbstractValidator<UpdateGymClassCommand>
{
    public UpdateGymClassCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("La descripción no puede superar los 500 caracteres.")
            .When(x => x.Description is not null);

        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0).WithMessage("La duración debe ser mayor a cero.");

        RuleFor(x => x.MaxCapacity)
            .GreaterThan(0).WithMessage("La capacidad máxima debe ser mayor a cero.");

        RuleFor(x => x.Color)
            .Matches(@"^#?[0-9A-Fa-f]{6}$").WithMessage("El color debe ser un valor hexadecimal válido.")
            .When(x => !string.IsNullOrWhiteSpace(x.Color));
    }
}
