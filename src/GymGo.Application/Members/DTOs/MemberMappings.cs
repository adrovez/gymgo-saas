using GymGo.Domain.Members;

namespace GymGo.Application.Members.DTOs;

/// <summary>
/// Métodos de mapeo estáticos de la entidad <see cref="Member"/> a los DTOs de lectura.
/// Se usan directamente en los QueryHandlers para evitar dependencias de librerías de mapeo
/// en la capa Domain y mantener control explícito sobre la proyección.
/// </summary>
public static class MemberMappings
{
    public static MemberDto ToDto(this Member m) => new(
        Id: m.Id,
        TenantId: m.TenantId,
        Rut: m.Rut,
        FirstName: m.FirstName,
        LastName: m.LastName,
        FullName: $"{m.FirstName} {m.LastName}",
        BirthDate: m.BirthDate,
        Age: CalculateAge(m.BirthDate),
        Gender: m.Gender,
        GenderLabel: m.Gender.ToLabel(),
        Email: m.Email,
        Phone: m.Phone,
        Address: m.Address,
        EmergencyContactName: m.EmergencyContactName,
        EmergencyContactPhone: m.EmergencyContactPhone,
        Status: m.Status,
        StatusLabel: m.Status.ToLabel(),
        RegistrationDate: m.RegistrationDate,
        Notes: m.Notes,
        CreatedAtUtc: m.CreatedAtUtc,
        CreatedBy: m.CreatedBy,
        ModifiedAtUtc: m.ModifiedAtUtc,
        ModifiedBy: m.ModifiedBy
    );

    public static MemberSummaryDto ToSummaryDto(this Member m) => new(
        Id: m.Id,
        Rut: m.Rut,
        FullName: $"{m.FirstName} {m.LastName}",
        Email: m.Email,
        Phone: m.Phone,
        Status: m.Status,
        StatusLabel: m.Status.ToLabel(),
        RegistrationDate: m.RegistrationDate
    );

    private static int CalculateAge(DateOnly birthDate)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var age = today.Year - birthDate.Year;
        if (birthDate > today.AddYears(-age)) age--;
        return age;
    }

    private static string ToLabel(this MemberStatus status) => status switch
    {
        MemberStatus.Active     => "Activo",
        MemberStatus.Suspended  => "Suspendido",
        MemberStatus.Delinquent => "Moroso",
        _                       => status.ToString()
    };

    private static string ToLabel(this Gender gender) => gender switch
    {
        Gender.Male          => "Masculino",
        Gender.Female        => "Femenino",
        Gender.Other         => "Otro",
        Gender.NotSpecified  => "No especificado",
        _                    => gender.ToString()
    };
}
