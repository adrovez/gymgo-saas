using FluentValidation;

namespace GymGo.Application.WorkoutLogs.Commands.CreateWorkoutLog;

public sealed class CreateWorkoutLogCommandValidator : AbstractValidator<CreateWorkoutLogCommand>
{
    public CreateWorkoutLogCommandValidator()
    {
        RuleFor(x => x.MemberId)
            .NotEmpty().WithMessage("El socio es obligatorio.");

        RuleFor(x => x.Title)
            .MaximumLength(200).WithMessage("El título no puede superar los 200 caracteres.")
            .When(x => x.Title is not null);

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Las observaciones no pueden superar los 1000 caracteres.")
            .When(x => x.Notes is not null);

        RuleFor(x => x.Date)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1))
            .WithMessage("La fecha de la sesión no puede ser futura.")
            .When(x => x.Date.HasValue);
    }
}
