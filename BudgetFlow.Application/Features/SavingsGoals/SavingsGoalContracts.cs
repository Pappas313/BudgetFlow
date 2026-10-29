namespace BudgetFlow.Application.Features.SavingsGoals;

/// <summary>
/// Data transfer object representing a savings goal owned by the current user, including
/// derived progress metrics.
/// </summary>
public sealed record SavingsGoalDto(
    Guid Id,
    string Name,
    decimal TargetAmount,
    decimal CurrentAmount,
    decimal ProgressPercentage,
    DateTimeOffset? TargetDateUtc,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? LastModifiedAtUtc);

/// <summary>
/// Data transfer object representing a single contribution made toward a savings goal.
/// </summary>
public sealed record SavingsContributionDto(Guid Id, Guid SavingsGoalId, decimal Amount, DateTimeOffset ContributedAtUtc);

/// <summary>
/// Request payload used to create a new savings goal.
/// </summary>
public sealed record CreateSavingsGoalRequest(string Name, decimal TargetAmount, DateTimeOffset? TargetDateUtc);

/// <summary>
/// Request payload used to record a contribution toward a savings goal.
/// </summary>
public sealed record RecordSavingsContributionRequest(decimal Amount, DateTimeOffset ContributedAtUtc);
