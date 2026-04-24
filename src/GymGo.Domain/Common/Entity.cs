namespace GymGo.Domain.Common;

/// <summary>
/// Entidad base con identidad. Por convención usamos Guid como Id
/// para evitar coordinación entre tenants.
/// </summary>
public abstract class Entity
{
    public Guid Id { get; protected set; }

    protected Entity() { }
    protected Entity(Guid id) => Id = id;

    public override bool Equals(object? obj)
    {
        if (obj is not Entity other) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;
        if (Id == Guid.Empty || other.Id == Guid.Empty) return false;
        return Id == other.Id;
    }

    public static bool operator ==(Entity? a, Entity? b)
        => a is null ? b is null : a.Equals(b);

    public static bool operator !=(Entity? a, Entity? b) => !(a == b);

    public override int GetHashCode() => (GetType().ToString() + Id).GetHashCode();
}
