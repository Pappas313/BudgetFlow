using BudgetFlow.Domain.Common;
using BudgetFlow.Domain.Enums;

namespace BudgetFlow.Domain.Entities;

/// <summary>
/// Represents a bank account owned by an application user.
/// </summary>
public class BankAccount : Entity, IAuditableEntity, ISoftDeletable
{
    public Guid ApplicationUserId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? Institution { get; private set; }

    public AccountType AccountType { get; private set; }

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

    public BankAccount(
        Guid applicationUserId,
        string name,
        string currency,
        AccountType accountType = AccountType.Checking,
        string? institution = null,
        decimal openingBalance = 0m)
    {
        ApplicationUserId = applicationUserId;
        Name = name;
        Currency = currency;
        AccountType = accountType;
        Institution = institution;
        CurrentBalance = openingBalance;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Updates the mutable details of the account. Ownership must be validated by the caller.
    /// </summary>
    public void UpdateDetails(string name, string currency, AccountType accountType, string? institution)
    {
        Name = name;
        Currency = currency;
        AccountType = accountType;
        Institution = institution;
        LastModifiedAtUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Logically deletes (archives) the account, preserving historical data.
    /// </summary>
    public void Archive()
    {
        if (IsDeleted)
        {
            return;
        }

        IsDeleted = true;
        DeletedAtUtc = DateTimeOffset.UtcNow;
        LastModifiedAtUtc = DeletedAtUtc;
    }
}
