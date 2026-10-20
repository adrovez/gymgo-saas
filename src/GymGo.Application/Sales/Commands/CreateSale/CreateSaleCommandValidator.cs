using FluentValidation;

namespace GymGo.Application.Sales.Commands.CreateSale;

public sealed class CreateSaleCommandValidator : AbstractValidator<CreateSaleCommand>
{
    public CreateSaleCommandValidator()
    {
        RuleFor(x => x.MemberId)
            .NotEmpty().WithMessage("El socio es obligatorio.");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Las observaciones no pueden superar los 500 caracteres.");
    }
}
