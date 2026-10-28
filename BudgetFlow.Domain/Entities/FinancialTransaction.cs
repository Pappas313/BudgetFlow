using BudgetFlow.Domain.Common;

namespace BudgetFlow.Domain.Entities;

/// <summary>
/// Represents a single financial transaction (income or expense) on a bank account.
/// </summary>
public class FinancialTransaction : Entity, IAuditableEntity, ISoftDeletable
{
    public Guid BankAccountId { get; private set; }

    public BankAccount? BankAccount { get; private set; }

    public Guid TransactionCategoryId { get; private set; }

    public TransactionCategory? TransactionCategory { get; private set; }

    public decimal Amount { get; private set; }

    public string? Description { get; private set; }

    public DateTimeOffset OccurredAtUtc { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? LastModifiedAtUtc { get; private set; }

    public bool IsDeleted { get; private set; }

    public DateTimeOffset? DeletedAtUtc { get; private set; }

    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    private FinancialTransaction()
    {
    }

    public FinancialTransaction(Guid bankAccountId, Guid transactionCategoryId, decimal amount, DateTimeOffset occurredAtUtc, string? description = null)
    {
        BankAccountId = bankAccountId;
        TransactionCategoryId = transactionCategoryId;
        Amount = amount;
        OccurredAtUtc = occurredAtUtc;
        Description = description;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }
}
