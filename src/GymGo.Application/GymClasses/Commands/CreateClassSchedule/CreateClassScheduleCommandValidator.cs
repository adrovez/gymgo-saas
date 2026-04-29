using FluentValidation;

namespace GymGo.Application.GymClasses.Commands.CreateClassSchedule;

public sealed class CreateClassScheduleCommandValidator : AbstractValidator<CreateClassScheduleCommand>
{
    public CreateClassScheduleCommandValidator()
    {
        RuleFor(x => x.GymClassId).NotEmpty();

        RuleFor(x => x.DayOfWeek)
            .InclusiveBetween(0, 6).WithMessage("El día de la semana debe ser entre 0 (Domingo) y 6 (Sábado).");

        RuleFor(x => x.StartTime)
            .NotEmpty().WithMessage("La hora de inicio es obligatoria.")
            .Matches(@"^([01]\d|2[0-3]):[0-5]\d$").WithMessage("Hora de inicio inválida (formato HH:mm).");

        RuleFor(x => x.EndTime)
            .NotEmpty().WithMessage("La hora de término es obligatoria.")
            .Matches(@"^([01]\d|2[0-3]):[0-5]\d$").WithMessage("Hora de término inválida (formato HH:mm).");

        RuleFor(x => x)
            .Must(x => string.Compare(x.EndTime, x.StartTime, StringComparison.Ordinal) > 0)
            .WithMessage("La hora de término debe ser posterior a la hora de inicio.")
            .When(x => !string.IsNullOrEmpty(x.StartTime) && !string.IsNullOrEmpty(x.EndTime));

        RuleFor(x => x.InstructorName).MaximumLength(100).When(x => x.InstructorName is not null);
        RuleFor(x => x.Room).MaximumLength(100).When(x => x.Room is not null);

        RuleFor(x => x.MaxCapacity)
            .GreaterThan(0).WithMessage("La capacidad máxima debe ser mayor a cero.")
            .When(x => x.MaxCapacity.HasValue);
    }
}
