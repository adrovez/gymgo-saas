using FluentValidation;

namespace GymGo.Application.Users.Commands.ChangeUserPassword;

/// <summary>
/// Validaciones de entrada para <see cref="ChangeUserPasswordCommand"/>.
/// </summary>
public sealed class ChangeUserPasswordCommandValidator : AbstractValidator<ChangeUserPasswordCommand>
{
    public ChangeUserPasswordCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("El Id del usuario es obligatorio.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("La nueva contraseña es obligatoria.")
            .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres.");
    }
}
