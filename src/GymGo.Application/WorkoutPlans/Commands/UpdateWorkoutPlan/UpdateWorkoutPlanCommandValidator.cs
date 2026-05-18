using FluentValidation;

namespace GymGo.Application.WorkoutPlans.Commands.UpdateWorkoutPlan;

public sealed class UpdateWorkoutPlanCommandValidator : AbstractValidator<UpdateWorkoutPlanCommand>
{
    public UpdateWorkoutPlanCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Objective)
            .NotEmpty().WithMessage("El objetivo es obligatorio.")
            .MaximumLength(500).WithMessage("El objetivo no puede superar los 500 caracteres.");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("La fecha de fin debe ser posterior o igual a la de inicio.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).When(x => x.Notes is not null);

        RuleFor(x => x.InitialWeightKg).GreaterThan(0).When(x => x.InitialWeightKg.HasValue);
        RuleFor(x => x.InitialHeightCm).GreaterThan(0).When(x => x.InitialHeightCm.HasValue);
        RuleFor(x => x.InitialBodyFatPercentage).InclusiveBetween(0, 100).When(x => x.InitialBodyFatPercentage.HasValue);
    }
}
