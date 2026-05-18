using FluentValidation;

namespace GymGo.Application.WorkoutLogs.Commands.AddExercise;

public sealed class AddExerciseCommandValidator : AbstractValidator<AddExerciseCommand>
{
    public AddExerciseCommandValidator()
    {
        RuleFor(x => x.WorkoutLogId).NotEmpty().WithMessage("El Id de la sesión es obligatorio.");

        RuleFor(x => x.ExerciseName)
            .NotEmpty().WithMessage("El nombre del ejercicio es obligatorio.")
            .MaximumLength(200).WithMessage("El nombre no puede superar los 200 caracteres.");

        RuleFor(x => x.MuscleGroup).IsInEnum();

        RuleFor(x => x.ActualSets).GreaterThan(0).When(x => x.ActualSets.HasValue);
        RuleFor(x => x.ActualReps).GreaterThan(0).When(x => x.ActualReps.HasValue);
        RuleFor(x => x.ActualWeightKg).GreaterThanOrEqualTo(0).When(x => x.ActualWeightKg.HasValue);
        RuleFor(x => x.ActualDurationMinutes).GreaterThan(0).When(x => x.ActualDurationMinutes.HasValue);
        RuleFor(x => x.ActualDistanceMeters).GreaterThan(0).When(x => x.ActualDistanceMeters.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(500).When(x => x.Notes is not null);
    }
}
