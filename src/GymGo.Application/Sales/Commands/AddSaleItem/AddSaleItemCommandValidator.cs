using FluentValidation;
using GymGo.Domain.Sales;

namespace GymGo.Application.Sales.Commands.AddSaleItem;

public sealed class AddSaleItemCommandValidator : AbstractValidator<AddSaleItemCommand>
{
    public AddSaleItemCommandValidator()
    {
        RuleFor(x => x.SaleId).NotEmpty().WithMessage("La venta es obligatoria.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("La descripción del ítem es obligatoria.")
            .MaximumLength(200).WithMessage("La descripción no puede superar los 200 caracteres.");

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0).WithMessage("El precio unitario debe ser mayor a cero.");

        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(1).WithMessage("La cantidad debe ser al menos 1.");

        RuleFor(x => x.MembershipPlanId)
            .NotEmpty()
            .When(x => x.ItemType == SaleItemType.Plan)
            .WithMessage("El ítem de tipo Plan requiere indicar el plan de membresía.");
    }
}
