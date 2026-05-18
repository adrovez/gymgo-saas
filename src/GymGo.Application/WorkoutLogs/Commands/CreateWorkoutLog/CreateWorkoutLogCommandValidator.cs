using FluentValidation;

namespace GymGo.Application.WorkoutLogs.Commands.CreateWorkoutLog;

public sealed class CreateWorkoutLogCommandValidator : AbstractValidator<CreateWorkoutLogCommand>
{
    public CreateWorkoutLogCommandValidator()
    {
        RuleFor(x => x.MemberId).NotEmpty().WithMessage("El socio es obligatorio.");
        RuleFor(x => x.WorkoutPlanId).NotEmpty().WithMessage("La rutina es obligatoria.");
        RuleFor(x => x.WorkoutPlanDayId).NotEmpty().WithMessage("El día de rutina es obligatorio.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Las observaciones no pueden superar los 1000 caracteres.")
            .When(x => x.Notes is not null);
    }
}
