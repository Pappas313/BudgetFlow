using BudgetFlow.Domain.Common;

namespace BudgetFlow.Domain.Entities;

/// <summary>
/// Represents a savings goal that the application user is contributing toward.
/// </summary>
public class SavingsGoal : Entity, IAuditableEntity, ISoftDeletable
{
    public Guid ApplicationUserId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public decimal TargetAmount { get; private set; }

    public decimal CurrentAmount { get; private set; }

    public DateTimeOffset? TargetDateUtc { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? LastModifiedAtUtc { get; private set; }

    public bool IsDeleted { get; private set; }

    public DateTimeOffset? DeletedAtUtc { get; private set; }

    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    public ICollection<SavingsContribution> SavingsContributions { get; private set; } = new List<SavingsContribution>();

    private SavingsGoal()
    {
    }

    public SavingsGoal(Guid applicationUserId, string name, decimal targetAmount, DateTimeOffset? targetDateUtc = null)
    {
        ApplicationUserId = applicationUserId;
        Name = name;
        TargetAmount = targetAmount;
        TargetDateUtc = targetDateUtc;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Records a contribution toward the goal, increasing the current saved amount.
    /// </summary>
    public void RecordContribution(decimal amount)
    {
        CurrentAmount += amount;
        LastModifiedAtUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Logically deletes (archives) the savings goal, preserving contribution history.
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
