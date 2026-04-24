namespace GymGo.Domain.Common;

/// <summary>
/// Marker interface para eventos de dominio. El dominio NO conoce MediatR;
/// la capa de Application define el adapter que los publica.
/// </summary>
public interface IDomainEvent
{
    DateTime OccurredOnUtc { get; }
}
