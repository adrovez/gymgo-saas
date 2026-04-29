using FluentValidation;
using GymGo.Domain.Maintenance;

namespace GymGo.Application.Maintenance.Commands.CreateMaintenanceRecord;

public sealed class CreateMaintenanceRecordCommandValidator
    : AbstractValidator<CreateMaintenanceRecordCommand>
{
    public CreateMaintenanceRecordCommandValidator()
    {
        RuleFor(x => x.EquipmentId)
            .NotEmpty().WithMessage("La máquina es obligatoria.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("El tipo de mantención no es válido.");

        RuleFor(x => x.ScheduledDate)
            .NotEmpty().WithMessage("La fecha programada es obligatoria.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("La descripción es obligatoria.")
            .MaximumLength(500).WithMessage("La descripción no puede superar los 500 caracteres.");

        RuleFor(x => x.ResponsibleType)
            .IsInEnum().WithMessage("El tipo de responsable no es válido.");

        // Responsable interno: userId requerido
        RuleFor(x => x.ResponsibleUserId)
            .NotEmpty().WithMessage("Debe indicar el usuario responsable para una mantención interna.")
            .When(x => x.ResponsibleType == ResponsibleType.Internal);

        // Responsable externo: nombre de proveedor requerido
        RuleFor(x => x.ExternalProviderName)
            .NotEmpty().WithMessage("Debe indicar el nombre del proveedor para una mantención externa.")
            .MaximumLength(200).WithMessage("El nombre del proveedor no puede superar los 200 caracteres.")
            .When(x => x.ResponsibleType == ResponsibleType.External);

        RuleFor(x => x.ExternalProviderContact)
            .MaximumLength(200).WithMessage("El contacto del proveedor no puede superar los 200 caracteres.")
            .When(x => x.ExternalProviderContact is not null);
    }
}
