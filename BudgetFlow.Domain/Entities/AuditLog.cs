using BudgetFlow.Domain.Common;

namespace BudgetFlow.Domain.Entities;

/// <summary>
/// Represents an immutable audit trail entry describing a change made within the system.
/// </summary>
public class AuditLog : Entity
{
    public Guid? ApplicationUserId { get; private set; }

    public string EntityName { get; private set; } = string.Empty;

    public string EntityId { get; private set; } = string.Empty;

    public string Action { get; private set; } = string.Empty;

    public string? Changes { get; private set; }

    public DateTimeOffset OccurredAtUtc { get; private set; }

    private AuditLog()
    {
    }

    public AuditLog(string entityName, string entityId, string action, string? changes, Guid? applicationUserId = null)
    {
        EntityName = entityName;
        EntityId = entityId;
        Action = action;
        Changes = changes;
        ApplicationUserId = applicationUserId;
        OccurredAtUtc = DateTimeOffset.UtcNow;
    }
}
