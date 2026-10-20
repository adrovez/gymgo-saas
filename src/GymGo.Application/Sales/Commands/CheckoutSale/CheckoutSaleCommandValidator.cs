using FluentValidation;
using GymGo.Domain.Sales;

namespace GymGo.Application.Sales.Commands.CheckoutSale;

public sealed class CheckoutSaleCommandValidator : AbstractValidator<CheckoutSaleCommand>
{
    public CheckoutSaleCommandValidator()
    {
        RuleFor(x => x.MemberId)
            .NotEmpty().WithMessage("El socio es obligatorio.");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Las observaciones no pueden superar los 500 caracteres.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("La venta debe tener al menos un concepto.");

        RuleFor(x => x.Items)
            .Must(items => items.Count(i => i.ItemType == SaleItemType.Plan) <= 1)
            .WithMessage("Solo puede existir un ítem de tipo Plan por venta.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Description)
                .NotEmpty().WithMessage("La descripción del ítem es obligatoria.")
                .MaximumLength(200).WithMessage("La descripción no puede superar los 200 caracteres.");

            item.RuleFor(i => i.UnitPrice)
                .GreaterThan(0).WithMessage("El precio unitario debe ser mayor a cero.");

            item.RuleFor(i => i.Quantity)
                .GreaterThanOrEqualTo(1).WithMessage("La cantidad debe ser al menos 1.");

            item.RuleFor(i => i.MembershipPlanId)
                .NotEmpty()
                .When(i => i.ItemType == SaleItemType.Plan)
                .WithMessage("El ítem de tipo Plan requiere indicar el plan de membresía.");

            item.RuleFor(i => i.MembershipPlanId)
                .Empty()
                .When(i => i.ItemType != SaleItemType.Plan)
                .WithMessage("Solo un ítem de tipo Plan puede referenciar un plan de membresía.");
        });

        When(x => x.Discount is not null, () =>
        {
            RuleFor(x => x.Discount!.Description)
                .NotEmpty().WithMessage("La descripción del descuento es obligatoria.")
                .MaximumLength(200).WithMessage("La descripción no puede superar los 200 caracteres.");

            RuleFor(x => x.Discount!.Value)
                .GreaterThan(0).WithMessage("El valor del descuento debe ser mayor a cero.");

            RuleFor(x => x.Discount!.Value)
                .LessThanOrEqualTo(100)
                .When(x => x.Discount!.DiscountType == SaleDiscountType.Porcentaje)
                .WithMessage("El porcentaje de descuento no puede superar el 100%.");
        });
    }
}
