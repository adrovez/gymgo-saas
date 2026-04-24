using FluentValidation;

namespace GymGo.Application.MembershipPlans.Commands.UpdateMembershipPlan;

public sealed class UpdateMembershipPlanCommandValidator : AbstractValidator<UpdateMembershipPlanCommand>
{
    public UpdateMembershipPlanCommandValidator()
    {
        RuleFor(x => x.PlanId)
            .NotEmpty().WithMessage("El Id del plan es obligatorio.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del plan es obligatorio.")
            .MaximumLength(150).WithMessage("El nombre no puede superar los 150 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("La descripción no puede superar los 500 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.Periodicity)
            .IsInEnum().WithMessage("La periodicidad proporcionada no es válida.");

        RuleFor(x => x.DaysPerWeek)
            .InclusiveBetween(1, 7).WithMessage("Los días por semana deben estar entre 1 y 7.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("El monto debe ser mayor a cero.");

        RuleFor(x => x)
            .Must(x => !x.FixedDays || AnyDayMarked(x))
            .WithMessage("Si el plan tiene días rígidos, debe marcar al menos un día.")
            .WithName("FixedDays");

        RuleFor(x => x)
            .Must(x => !x.FixedDays || CountDays(x) == x.DaysPerWeek)
            .WithMessage("Los días marcados deben coincidir con la cantidad de días por semana.")
            .WithName("DaysPerWeek");

        RuleFor(x => x.TimeFrom)
            .NotNull().WithMessage("La hora de inicio es obligatoria cuando el horario no es libre.")
            .When(x => !x.FreeSchedule);

        RuleFor(x => x.TimeTo)
            .NotNull().WithMessage("La hora de fin es obligatoria cuando el horario no es libre.")
            .When(x => !x.FreeSchedule);

        RuleFor(x => x)
            .Must(x => x.TimeFrom < x.TimeTo)
            .WithMessage("La hora de inicio debe ser anterior a la hora de fin.")
            .WithName("TimeFrom")
            .When(x => !x.FreeSchedule && x.TimeFrom.HasValue && x.TimeTo.HasValue);
    }

    private static bool AnyDayMarked(UpdateMembershipPlanCommand x) =>
        x.Monday || x.Tuesday || x.Wednesday || x.Thursday || x.Friday || x.Saturday || x.Sunday;

    private static int CountDays(UpdateMembershipPlanCommand x)
    {
        var count = 0;
        if (x.Monday)    count++;
        if (x.Tuesday)   count++;
        if (x.Wednesday) count++;
        if (x.Thursday)  count++;
        if (x.Friday)    count++;
        if (x.Saturday)  count++;
        if (x.Sunday)    count++;
        return count;
    }
}
