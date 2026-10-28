using BudgetFlow.Domain.Common;

namespace BudgetFlow.Domain.Entities;

/// <summary>
/// Represents a user-defined category used to classify financial transactions.
/// </summary>
public class TransactionCategory : Entity, IAuditableEntity, ISoftDeletable
{
    public Guid ApplicationUserId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? LastModifiedAtUtc { get; private set; }

    public bool IsDeleted { get; private set; }

    public DateTimeOffset? DeletedAtUtc { get; private set; }

    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    public ICollection<FinancialTransaction> FinancialTransactions { get; private set; } = new List<FinancialTransaction>();

    private TransactionCategory()
    {
    }

    public TransactionCategory(Guid applicationUserId, string name, string? description = null)
    {
        ApplicationUserId = applicationUserId;
        Name = name;
        Description = description;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }
}
