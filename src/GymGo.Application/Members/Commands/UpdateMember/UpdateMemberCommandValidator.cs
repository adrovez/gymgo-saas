using FluentValidation;

namespace GymGo.Application.Members.Commands.UpdateMember;

public sealed class UpdateMemberCommandValidator : AbstractValidator<UpdateMemberCommand>
{
    public UpdateMemberCommandValidator()
    {
        RuleFor(x => x.MemberId)
            .NotEmpty().WithMessage("El Id del socio es obligatorio.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("El apellido es obligatorio.")
            .MaximumLength(100).WithMessage("El apellido no puede superar los 100 caracteres.");

        RuleFor(x => x.BirthDate)
            .NotEmpty().WithMessage("La fecha de nacimiento es obligatoria.")
            .LessThan(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("La fecha de nacimiento debe ser anterior a la fecha actual.");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("El email no tiene un formato válido.")
            .MaximumLength(200).WithMessage("El email no puede superar los 200 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Phone)
            .MaximumLength(40).WithMessage("El celular no puede superar los 40 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.Address)
            .MaximumLength(300).WithMessage("La dirección no puede superar los 300 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Address));

        RuleFor(x => x.EmergencyContactName)
            .MaximumLength(200)
            .WithMessage("El nombre del contacto de emergencia no puede superar los 200 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.EmergencyContactName));

        RuleFor(x => x.EmergencyContactPhone)
            .MaximumLength(40)
            .WithMessage("El teléfono del contacto de emergencia no puede superar los 40 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.EmergencyContactPhone));

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Las observaciones no pueden superar los 1000 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}
