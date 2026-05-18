using FluentValidation;

namespace GymGo.Application.WorkoutLogs.Commands.UpdateWorkoutLog;

public sealed class UpdateWorkoutLogCommandValidator : AbstractValidator<UpdateWorkoutLogCommand>
{
    public UpdateWorkoutLogCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Las observaciones no pueden superar los 1000 caracteres.")
            .When(x => x.Notes is not null);
    }
}
