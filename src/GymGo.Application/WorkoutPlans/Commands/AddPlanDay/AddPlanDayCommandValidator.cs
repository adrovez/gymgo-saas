using FluentValidation;
using GymGo.Domain.WorkoutLogs;

namespace GymGo.Application.WorkoutPlans.Commands.AddPlanDay;

public sealed class AddPlanDayCommandValidator : AbstractValidator<AddPlanDayCommand>
{
    public AddPlanDayCommandValidator()
    {
        RuleFor(x => x.WorkoutPlanId).NotEmpty().WithMessage("La rutina es obligatoria.");

        RuleFor(x => x.DayOfWeek)
            .IsInEnum().WithMessage("El día de la semana no es válido.");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Las notas no pueden superar los 500 caracteres.")
            .When(x => x.Notes is not null);
    }
}
