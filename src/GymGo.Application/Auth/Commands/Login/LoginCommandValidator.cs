using FluentValidation;

namespace GymGo.Application.Auth.Commands.Login;

/// <summary>
/// Validaciones de formato/presencia para <see cref="LoginCommand"/>.
/// La verificación de contraseña y estado del usuario se realiza en el handler.
/// </summary>
public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es obligatorio.")
            .EmailAddress().WithMessage("El email no tiene un formato válido.")
            .MaximumLength(200).WithMessage("El email no puede superar los 200 caracteres.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es obligatoria.")
            .MaximumLength(200).WithMessage("La contraseña no puede superar los 200 caracteres.");
    }
}
