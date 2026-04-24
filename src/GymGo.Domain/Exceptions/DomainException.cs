namespace GymGo.Domain.Exceptions;

/// <summary>
/// Excepción base para violaciones de invariantes de dominio.
/// La capa API la traduce a 422 (Unprocessable Entity).
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
    protected DomainException(string message, Exception inner) : base(message, inner) { }
}
