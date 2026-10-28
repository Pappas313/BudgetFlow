using BudgetFlow.Domain.Common;

namespace BudgetFlow.Domain.Entities;

/// <summary>
/// Represents a bank account owned by an application user.
/// </summary>
public class BankAccount : Entity, IAuditableEntity, ISoftDeletable
{
    public Guid ApplicationUserId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? Institution { get; private set; }

    public string Currency { get; private set; } = "USD";

    public decimal CurrentBalance { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? LastModifiedAtUtc { get; private set; }

    public bool IsDeleted { get; private set; }

    public DateTimeOffset? DeletedAtUtc { get; private set; }

    /// <summary>
    /// Concurrency token used by EF Core to detect conflicting updates.
    /// </summary>
    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    public ICollection<FinancialTransaction> FinancialTransactions { get; private set; } = new List<FinancialTransaction>();

    private BankAccount()
    {
    }

    public BankAccount(Guid applicationUserId, string name, string currency, string? institution = null)
    {
        ApplicationUserId = applicationUserId;
        Name = name;
        Currency = currency;
        Institution = institution;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }
}
