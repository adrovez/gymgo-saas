using FluentValidation;
using GymGo.Domain.Members;

namespace GymGo.Application.Members.Commands.ChangeMemberStatus;

public sealed class ChangeMemberStatusCommandValidator : AbstractValidator<ChangeMemberStatusCommand>
{
    public ChangeMemberStatusCommandValidator()
    {
        RuleFor(x => x.MemberId)
            .NotEmpty().WithMessage("El Id del socio es obligatorio.");

        RuleFor(x => x.NewStatus)
            .IsInEnum().WithMessage("El estado proporcionado no es válido.");
    }
}
