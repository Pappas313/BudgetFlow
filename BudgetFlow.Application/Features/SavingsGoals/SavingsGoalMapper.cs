using BudgetFlow.Domain.Entities;

namespace BudgetFlow.Application.Features.SavingsGoals;

internal static class SavingsGoalMapper
{
    public static SavingsGoalDto ToDto(SavingsGoal goal)
    {
        var progress = goal.TargetAmount == 0 ? 0 : goal.CurrentAmount / goal.TargetAmount * 100m;

        return new SavingsGoalDto(
            goal.Id,
            goal.Name,
            goal.TargetAmount,
            goal.CurrentAmount,
            progress,
            goal.TargetDateUtc,
            goal.CreatedAtUtc,
            goal.LastModifiedAtUtc);
    }

    public static SavingsContributionDto ToDto(SavingsContribution contribution) => new(
        contribution.Id,
        contribution.SavingsGoalId,
        contribution.Amount,
        contribution.ContributedAtUtc);
}
