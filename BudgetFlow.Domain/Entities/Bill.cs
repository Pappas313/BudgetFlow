using BudgetFlow.Domain.Common;

namespace BudgetFlow.Domain.Entities;

/// <summary>
/// Represents a recurring or one-time bill owed by the application user.
/// </summary>
public class Bill : Entity, IAuditableEntity, ISoftDeletable
{
    public Guid ApplicationUserId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public decimal Amount { get; private set; }

    public DateTimeOffset DueDateUtc { get; private set; }

    public bool IsRecurring { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? LastModifiedAtUtc { get; private set; }

    public bool IsDeleted { get; private set; }

    public DateTimeOffset? DeletedAtUtc { get; private set; }

    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    public ICollection<BillPayment> BillPayments { get; private set; } = new List<BillPayment>();

    private Bill()
    {
    }

    public Bill(Guid applicationUserId, string name, decimal amount, DateTimeOffset dueDateUtc, bool isRecurring = false)
    {
        ApplicationUserId = applicationUserId;
        Name = name;
        Amount = amount;
        DueDateUtc = dueDateUtc;
        IsRecurring = isRecurring;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Updates the mutable details of the bill. Ownership must be validated by the caller.
    /// </summary>
    public void UpdateDetails(string name, decimal amount, DateTimeOffset dueDateUtc, bool isRecurring)
    {
        Name = name;
        Amount = amount;
        DueDateUtc = dueDateUtc;
        IsRecurring = isRecurring;
        LastModifiedAtUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Logically deletes (archives) the bill, preserving its payment history.
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
