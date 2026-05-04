using FluentValidation;

namespace GymGo.Application.WorkoutLogs.Commands.UpdateWorkoutLog;

public sealed class UpdateWorkoutLogCommandValidator : AbstractValidator<UpdateWorkoutLogCommand>
{
    public UpdateWorkoutLogCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("El Id de la sesión es obligatorio.");

        RuleFor(x => x.Title)
            .MaximumLength(200).WithMessage("El título no puede superar los 200 caracteres.")
            .When(x => x.Title is not null);

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Las observaciones no pueden superar los 1000 caracteres.")
            .When(x => x.Notes is not null);
    }
}
