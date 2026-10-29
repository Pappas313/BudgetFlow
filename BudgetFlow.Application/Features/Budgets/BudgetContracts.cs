namespace BudgetFlow.Application.Features.Budgets;

/// <summary>
/// Data transfer object representing a monthly category budget along with computed actual
/// spending and derived financial metrics.
/// </summary>
public sealed record MonthlyBudgetDto(
    Guid Id,
    Guid TransactionCategoryId,
    string CategoryName,
    int Year,
    int Month,
    decimal AllocatedBudget,
    decimal ActualExpenses,
    decimal RemainingBudget,
    decimal BudgetUtilization,
    decimal BudgetVariance,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? LastModifiedAtUtc);

/// <summary>
/// Aggregated summary across all monthly budgets for a given year and month.
/// </summary>
public sealed record BudgetSummaryDto(
    int Year,
    int Month,
    decimal TotalAllocatedBudget,
    decimal TotalActualExpenses,
    decimal TotalRemainingBudget,
    decimal OverallBudgetUtilization,
    IReadOnlyList<MonthlyBudgetDto> Budgets);

public sealed record CreateMonthlyBudgetRequest(Guid TransactionCategoryId, int Year, int Month, decimal BudgetedAmount);

public sealed record UpdateMonthlyBudgetRequest(decimal BudgetedAmount);
