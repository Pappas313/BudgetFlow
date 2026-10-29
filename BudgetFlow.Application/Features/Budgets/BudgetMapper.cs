using BudgetFlow.Domain.Entities;

namespace BudgetFlow.Application.Features.Budgets;

internal static class BudgetMapper
{
    /// <summary>
    /// Computes the derived financial metrics for a monthly budget:
    /// RemainingBudget = AllocatedBudget - ActualExpenses
    /// BudgetUtilization = ActualExpenses / AllocatedBudget * 100
    /// BudgetVariance = AllocatedBudget - ActualExpenses (positive = under budget, negative = over budget)
    /// </summary>
    public static MonthlyBudgetDto ToDto(MonthlyBudget budget, decimal actualExpenses)
    {
        var allocated = budget.BudgetedAmount;
        var remaining = allocated - actualExpenses;
        var utilization = allocated == 0 ? 0 : actualExpenses / allocated * 100m;
        var variance = allocated - actualExpenses;

        return new MonthlyBudgetDto(
            budget.Id,
            budget.TransactionCategoryId,
            budget.TransactionCategory?.Name ?? string.Empty,
            budget.Year,
            budget.Month,
            allocated,
            actualExpenses,
            remaining,
            utilization,
            variance,
            budget.CreatedAtUtc,
            budget.LastModifiedAtUtc);
    }
}
