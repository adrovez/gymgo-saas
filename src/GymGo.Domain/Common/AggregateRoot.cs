namespace GymGo.Domain.Common;

/// <summary>
/// Raíz de agregado. Acumula eventos de dominio que se publican
/// al hacer SaveChanges en el DbContext.
/// </summary>
public abstract class AggregateRoot : Entity
{
    private readonly List<IDomainEvent> _domainEvents = new();

    protected AggregateRoot() { }
    protected AggregateRoot(Guid id) : base(id) { }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent @event) => _domainEvents.Add(@event);

    public void ClearDomainEvents() => _domainEvents.Clear();
}
