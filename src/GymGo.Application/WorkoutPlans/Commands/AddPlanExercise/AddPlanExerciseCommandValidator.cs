using FluentValidation;

namespace GymGo.Application.WorkoutPlans.Commands.AddPlanExercise;

public sealed class AddPlanExerciseCommandValidator : AbstractValidator<AddPlanExerciseCommand>
{
    public AddPlanExerciseCommandValidator()
    {
        RuleFor(x => x.WorkoutPlanDayId).NotEmpty().WithMessage("El día de rutina es obligatorio.");

        RuleFor(x => x.ExerciseName)
            .NotEmpty().WithMessage("El nombre del ejercicio es obligatorio.")
            .MaximumLength(200).WithMessage("El nombre no puede superar los 200 caracteres.");

        RuleFor(x => x.MuscleGroup).IsInEnum().WithMessage("El grupo muscular no es válido.");

        RuleFor(x => x.PlannedSets).GreaterThan(0).When(x => x.PlannedSets.HasValue);
        RuleFor(x => x.PlannedReps).GreaterThan(0).When(x => x.PlannedReps.HasValue);
        RuleFor(x => x.PlannedWeightKg).GreaterThanOrEqualTo(0).When(x => x.PlannedWeightKg.HasValue);
        RuleFor(x => x.PlannedDurationMinutes).GreaterThan(0).When(x => x.PlannedDurationMinutes.HasValue);
        RuleFor(x => x.PlannedDistanceMeters).GreaterThan(0).When(x => x.PlannedDistanceMeters.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Las notas no pueden superar los 500 caracteres.")
            .When(x => x.Notes is not null);
    }
}
