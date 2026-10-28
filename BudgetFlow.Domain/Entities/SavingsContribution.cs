using BudgetFlow.Domain.Common;

namespace BudgetFlow.Domain.Entities;

/// <summary>
/// Represents a single contribution made toward a <see cref="SavingsGoal"/>.
/// </summary>
public class SavingsContribution : Entity, IAuditableEntity
{
    public Guid SavingsGoalId { get; private set; }

    public SavingsGoal? SavingsGoal { get; private set; }

    public decimal Amount { get; private set; }

    public DateTimeOffset ContributedAtUtc { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? LastModifiedAtUtc { get; private set; }

    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    private SavingsContribution()
    {
    }

    public SavingsContribution(Guid savingsGoalId, decimal amount, DateTimeOffset contributedAtUtc)
    {
        SavingsGoalId = savingsGoalId;
        Amount = amount;
        ContributedAtUtc = contributedAtUtc;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }
}
