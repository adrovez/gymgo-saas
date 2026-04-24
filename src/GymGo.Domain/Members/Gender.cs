namespace GymGo.Domain.Members;

/// <summary>
/// Género del socio. Campo opcional, utilizado para estadísticas y personalización.
/// Persistido como INT en la base de datos.
/// </summary>
public enum Gender
{
    /// <summary>No especificado (valor por defecto).</summary>
    NotSpecified = 0,

    /// <summary>Masculino.</summary>
    Male = 1,

    /// <summary>Femenino.</summary>
    Female = 2,

    /// <summary>Otro / prefiero no indicar.</summary>
    Other = 3
}
