namespace GymGo.Domain.Exceptions;

/// <summary>
/// Violación de regla de negocio. Se traduce a HTTP 422.
/// </summary>
public sealed class BusinessRuleViolationException : DomainException
{
    public string Code { get; }

    public BusinessRuleViolationException(string code, string message)
        : base(message)
    {
        Code = code;
    }
}
