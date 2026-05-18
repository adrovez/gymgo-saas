using MediatR;

namespace GymGo.Application.WorkoutPlans.Commands.RemovePlanExercise;

public sealed record RemovePlanExerciseCommand(Guid WorkoutPlanDayId, Guid ExerciseId) : IRequest;
