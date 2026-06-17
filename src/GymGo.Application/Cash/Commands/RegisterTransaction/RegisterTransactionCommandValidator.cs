using FluentValidation;
using GymGo.Domain.Cash;

namespace GymGo.Application.Cash.Commands.RegisterTransaction;

public sealed class RegisterTransactionCommandValidator : AbstractValidator<RegisterTransactionCommand>
{
    private static readonly TransactionConcept[] IncomeConcepts =
    [
        TransactionConcept.CuotaMembresia,
        TransactionConcept.Matricula,
        TransactionConcept.ProductoServicio,
        TransactionConcept.OtroIngreso
    ];

    private static readonly TransactionConcept[] ExpenseConcepts =
    [
        TransactionConcept.Servicios,
        TransactionConcept.Mantencion,
        TransactionConcept.Insumos,
        TransactionConcept.OtroEgreso
    ];

    public RegisterTransactionCommandValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("El monto debe ser mayor a cero.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Tipo de transacción inválido.");

        RuleFor(x => x.PaymentMethod)
            .IsInEnum().WithMessage("Método de pago inválido.");

        RuleFor(x => x.Concept)
            .IsInEnum().WithMessage("Concepto inválido.");

        // Concepto coherente con el tipo
        RuleFor(x => x.Concept)
            .Must((cmd, concept) => IncomeConcepts.Contains(concept))
            .When(x => x.Type == CashTransactionType.Ingreso)
            .WithMessage("El concepto no corresponde a un ingreso.");

        RuleFor(x => x.Concept)
            .Must((cmd, concept) => ExpenseConcepts.Contains(concept))
            .When(x => x.Type == CashTransactionType.Egreso)
            .WithMessage("El concepto no corresponde a un egreso.");

        // Descripción obligatoria en egresos
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("La descripción es obligatoria para los egresos.")
            .When(x => x.Type == CashTransactionType.Egreso);

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("La descripción no puede superar los 500 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        // Socios solo en ingresos
        RuleFor(x => x.MemberId)
            .Null().WithMessage("Los egresos no pueden vincularse a un socio.")
            .When(x => x.Type == CashTransactionType.Egreso);

        RuleFor(x => x.MembershipAssignmentId)
            .Null().WithMessage("Los egresos no pueden vincularse a una membresía.")
            .When(x => x.Type == CashTransactionType.Egreso);
    }
}
