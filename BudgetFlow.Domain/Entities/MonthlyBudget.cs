using BudgetFlow.Domain.Common;

namespace BudgetFlow.Domain.Entities;

/// <summary>
/// Represents a budgeted amount for a specific category within a given month and year.
/// </summary>
public class MonthlyBudget : Entity, IAuditableEntity
{
    public Guid ApplicationUserId { get; private set; }

    public Guid TransactionCategoryId { get; private set; }

    public TransactionCategory? TransactionCategory { get; private set; }

    public int Year { get; private set; }

    public int Month { get; private set; }

    public decimal BudgetedAmount { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? LastModifiedAtUtc { get; private set; }

    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    private MonthlyBudget()
    {
    }

    public MonthlyBudget(Guid applicationUserId, Guid transactionCategoryId, int year, int month, decimal budgetedAmount)
    {
        ApplicationUserId = applicationUserId;
        TransactionCategoryId = transactionCategoryId;
        Year = year;
        Month = month;
        BudgetedAmount = budgetedAmount;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }
}
