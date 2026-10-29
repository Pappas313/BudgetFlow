using BudgetFlow.Domain.Common;
using BudgetFlow.Domain.Enums;

namespace BudgetFlow.Domain.Entities;

/// <summary>
/// Represents a single financial transaction (income, expense, or transfer) on a bank account.
/// </summary>
public class FinancialTransaction : Entity, IAuditableEntity, ISoftDeletable
{
    public Guid BankAccountId { get; private set; }

    public BankAccount? BankAccount { get; private set; }

    public Guid? TransactionCategoryId { get; private set; }

    public TransactionCategory? TransactionCategory { get; private set; }

    public TransactionType TransactionType { get; private set; }

    /// <summary>
    /// The signed amount applied to the owning account's balance. Positive values increase the
    /// balance (income, transfer credit); negative values decrease it (expense, transfer debit).
    /// </summary>
    public decimal Amount { get; private set; }

    public string? Description { get; private set; }

    public DateTimeOffset OccurredAtUtc { get; private set; }

    /// <summary>
    /// Links the two legs (debit/credit) of a transfer together. Null for income/expense transactions.
    /// </summary>
    public Guid? TransferGroupId { get; private set; }

    /// <summary>
    /// An optional caller-supplied identifier (e.g. from a bank import file) used to detect and
    /// prevent duplicate transactions from being recorded for the same account.
    /// </summary>
    public string? ExternalReference { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? LastModifiedAtUtc { get; private set; }

    public bool IsDeleted { get; private set; }

    public DateTimeOffset? DeletedAtUtc { get; private set; }

    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    private FinancialTransaction()
    {
    }

    public FinancialTransaction(
        Guid bankAccountId,
        TransactionType transactionType,
        decimal amount,
        DateTimeOffset occurredAtUtc,
        Guid? transactionCategoryId = null,
        string? description = null,
        string? externalReference = null,
        Guid? transferGroupId = null)
    {
        BankAccountId = bankAccountId;
        TransactionType = transactionType;
        TransactionCategoryId = transactionCategoryId;
        Amount = amount;
        OccurredAtUtc = occurredAtUtc;
        Description = description;
        ExternalReference = externalReference;
        TransferGroupId = transferGroupId;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Updates the mutable details of an income/expense transaction. Ownership and category
    /// ownership must be validated by the caller. Not applicable to transfer legs.
    /// </summary>
    public void UpdateDetails(Guid? transactionCategoryId, decimal amount, DateTimeOffset occurredAtUtc, string? description)
    {
        TransactionCategoryId = transactionCategoryId;
        Amount = amount;
        OccurredAtUtc = occurredAtUtc;
        Description = description;
        LastModifiedAtUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Logically deletes (archives) the transaction, preserving historical data.
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
