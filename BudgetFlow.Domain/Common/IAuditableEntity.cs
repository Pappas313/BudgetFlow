namespace BudgetFlow.Domain.Common;

/// <summary>
/// Marker interface applied to entities that must track creation and
/// last-modification timestamps, expressed in UTC.
/// </summary>
public interface IAuditableEntity
{
    /// <summary>
    /// The UTC timestamp at which the entity was created.
    /// </summary>
    DateTimeOffset CreatedAtUtc { get; }

    /// <summary>
    /// The UTC timestamp at which the entity was last modified, if any.
    /// </summary>
    DateTimeOffset? LastModifiedAtUtc { get; }
}
