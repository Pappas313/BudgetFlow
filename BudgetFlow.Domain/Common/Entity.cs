namespace BudgetFlow.Domain.Common;

/// <summary>
/// Base class for all domain entities that are identified by a unique <see cref="Guid"/>.
/// Entities are compared by identity rather than by their attribute values.
/// </summary>
public abstract class Entity : IEquatable<Entity>
{
    /// <summary>
    /// Gets the unique identifier of the entity.
    /// </summary>
    public Guid Id { get; protected set; }

    protected Entity()
    {
        Id = Guid.NewGuid();
    }

    protected Entity(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Entity identifier cannot be empty.", nameof(id));
        }

        Id = id;
    }

    public bool Equals(Entity? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (GetType() != other.GetType())
        {
            return false;
        }

        return Id == other.Id;
    }

    public override bool Equals(object? obj) => Equals(obj as Entity);

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    public static bool operator ==(Entity? left, Entity? right) =>
        left is null ? right is null : left.Equals(right);

    public static bool operator !=(Entity? left, Entity? right) => !(left == right);
}
