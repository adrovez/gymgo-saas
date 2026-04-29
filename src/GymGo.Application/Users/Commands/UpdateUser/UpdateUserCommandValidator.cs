using FluentValidation;
using GymGo.Domain.Users;

namespace GymGo.Application.Users.Commands.UpdateUser;

/// <summary>
/// Validaciones de entrada para <see cref="UpdateUserCommand"/>.
/// </summary>
public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("El Id del usuario es obligatorio.");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("El nombre completo es obligatorio.")
            .MinimumLength(2).WithMessage("El nombre debe tener al menos 2 caracteres.")
            .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres.");

        RuleFor(x => x.Role)
            .Must(r => r == UserRole.GymStaff || r == UserRole.Instructor)
            .WithMessage("Solo se pueden asignar los roles Staff e Instructor desde la gestión de usuarios.");
    }
}
