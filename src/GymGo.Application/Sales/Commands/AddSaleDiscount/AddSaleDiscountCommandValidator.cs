using FluentValidation;
using GymGo.Domain.Sales;

namespace GymGo.Application.Sales.Commands.AddSaleDiscount;

public sealed class AddSaleDiscountCommandValidator : AbstractValidator<AddSaleDiscountCommand>
{
    public AddSaleDiscountCommandValidator()
    {
        RuleFor(x => x.SaleId).NotEmpty().WithMessage("La venta es obligatoria.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("La descripción del descuento es obligatoria.")
            .MaximumLength(200).WithMessage("La descripción no puede superar los 200 caracteres.");

        RuleFor(x => x.Value)
            .GreaterThan(0).WithMessage("El valor del descuento debe ser mayor a cero.")
            .LessThanOrEqualTo(100)
            .When(x => x.DiscountType == SaleDiscountType.Porcentaje)
            .WithMessage("El porcentaje de descuento no puede superar el 100%.");
    }
}
