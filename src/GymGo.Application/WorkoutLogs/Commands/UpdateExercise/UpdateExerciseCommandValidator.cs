using FluentValidation;

namespace GymGo.Application.WorkoutLogs.Commands.UpdateExercise;

public sealed class UpdateExerciseCommandValidator : AbstractValidator<UpdateExerciseCommand>
{
    public UpdateExerciseCommandValidator()
    {
        RuleFor(x => x.WorkoutLogId)
            .NotEmpty().WithMessage("El Id de la sesión es obligatorio.");

        RuleFor(x => x.ExerciseId)
            .NotEmpty().WithMessage("El Id del ejercicio es obligatorio.");

        RuleFor(x => x.ExerciseName)
            .NotEmpty().WithMessage("El nombre del ejercicio es obligatorio.")
            .MaximumLength(200).WithMessage("El nombre del ejercicio no puede superar los 200 caracteres.");

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0).WithMessage("El orden debe ser un número no negativo.");

        RuleFor(x => x.Sets)
            .GreaterThan(0).WithMessage("El número de series debe ser mayor a cero.")
            .When(x => x.Sets.HasValue);

        RuleFor(x => x.Reps)
            .GreaterThan(0).WithMessage("Las repeticiones deben ser mayores a cero.")
            .When(x => x.Reps.HasValue);

        RuleFor(x => x.WeightKg)
            .GreaterThanOrEqualTo(0).WithMessage("El peso no puede ser negativo.")
            .When(x => x.WeightKg.HasValue);

        RuleFor(x => x.DurationSeconds)
            .GreaterThan(0).WithMessage("La duración debe ser mayor a cero segundos.")
            .When(x => x.DurationSeconds.HasValue);

        RuleFor(x => x.DistanceMeters)
            .GreaterThan(0).WithMessage("La distancia debe ser mayor a cero metros.")
            .When(x => x.DistanceMeters.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Las notas no pueden superar los 500 caracteres.")
            .When(x => x.Notes is not null);
    }
}
