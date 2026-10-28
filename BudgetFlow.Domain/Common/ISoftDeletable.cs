namespace BudgetFlow.Domain.Common;

/// <summary>
/// Marker interface applied to entities that support logical (soft) deletion
/// instead of being physically removed from the data store.
/// </summary>
public interface ISoftDeletable
{
    /// <summary>
    /// Indicates whether the entity has been logically deleted.
    /// </summary>
    bool IsDeleted { get; }

    /// <summary>
    /// The UTC timestamp at which the entity was logically deleted, if any.
    /// </summary>
    DateTimeOffset? DeletedAtUtc { get; }
}
