using FluentValidation;

namespace GymGo.Application.Equipment.Commands.UpdateEquipment;

public sealed class UpdateEquipmentCommandValidator : AbstractValidator<UpdateEquipmentCommand>
{
    public UpdateEquipmentCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("El Id es obligatorio.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres.");

        RuleFor(x => x.Brand)
            .MaximumLength(100).WithMessage("La marca no puede superar los 100 caracteres.")
            .When(x => x.Brand is not null);

        RuleFor(x => x.Model)
            .MaximumLength(100).WithMessage("El modelo no puede superar los 100 caracteres.")
            .When(x => x.Model is not null);

        RuleFor(x => x.SerialNumber)
            .MaximumLength(50).WithMessage("El número de serie no puede superar los 50 caracteres.")
            .When(x => x.SerialNumber is not null);

        RuleFor(x => x.ImageUrl)
            .MaximumLength(500).WithMessage("La URL de imagen no puede superar los 500 caracteres.")
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("La URL de imagen no tiene un formato válido.")
            .When(x => !string.IsNullOrWhiteSpace(x.ImageUrl));
    }
}
