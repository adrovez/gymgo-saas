using FluentValidation;

namespace GymGo.Application.WorkoutPlans.Commands.CreateWorkoutPlan;

public sealed class CreateWorkoutPlanCommandValidator : AbstractValidator<CreateWorkoutPlanCommand>
{
    public CreateWorkoutPlanCommandValidator()
    {
        RuleFor(x => x.MemberId).NotEmpty().WithMessage("El socio es obligatorio.");

        RuleFor(x => x.Objective)
            .NotEmpty().WithMessage("El objetivo es obligatorio.")
            .MaximumLength(500).WithMessage("El objetivo no puede superar los 500 caracteres.");

        RuleFor(x => x.StartDate).NotEmpty().WithMessage("La fecha de inicio es obligatoria.");
        RuleFor(x => x.EndDate).NotEmpty().WithMessage("La fecha de fin es obligatoria.");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("La fecha de fin debe ser posterior o igual a la fecha de inicio.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Las observaciones no pueden superar los 1000 caracteres.")
            .When(x => x.Notes is not null);

        RuleFor(x => x.InitialWeightKg).GreaterThan(0).When(x => x.InitialWeightKg.HasValue);
        RuleFor(x => x.InitialHeightCm).GreaterThan(0).When(x => x.InitialHeightCm.HasValue);
        RuleFor(x => x.InitialBodyFatPercentage)
            .InclusiveBetween(0, 100)
            .When(x => x.InitialBodyFatPercentage.HasValue);
    }
}
