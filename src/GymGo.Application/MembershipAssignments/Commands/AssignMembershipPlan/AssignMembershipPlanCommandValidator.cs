using FluentValidation;

namespace GymGo.Application.MembershipAssignments.Commands.AssignMembershipPlan;

public sealed class AssignMembershipPlanCommandValidator : AbstractValidator<AssignMembershipPlanCommand>
{
    public AssignMembershipPlanCommandValidator()
    {
        RuleFor(x => x.MemberId)
            .NotEmpty().WithMessage("El Id del socio es obligatorio.");

        RuleFor(x => x.MembershipPlanId)
            .NotEmpty().WithMessage("El Id del plan es obligatorio.");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Las observaciones no pueden superar los 500 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}
