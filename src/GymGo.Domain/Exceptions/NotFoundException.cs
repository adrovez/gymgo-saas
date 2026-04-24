namespace GymGo.Domain.Exceptions;

/// <summary>
/// Recurso de dominio no encontrado. Se traduce a HTTP 404.
/// </summary>
public sealed class NotFoundException : DomainException
{
    public string EntityName { get; }
    public object Key { get; }

    public NotFoundException(string entityName, object key)
        : base($"La entidad '{entityName}' con clave '{key}' no fue encontrada.")
    {
        EntityName = entityName;
        Key = key;
    }
}
