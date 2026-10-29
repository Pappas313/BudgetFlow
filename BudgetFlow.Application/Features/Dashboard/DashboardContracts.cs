namespace BudgetFlow.Application.Features.Dashboard;

/// <summary>
/// High-level financial overview for the current user: total balances across accounts,
/// current-month income/expense/net cash flow, and overall budget utilization.
/// </summary>
public sealed record DashboardSummaryDto(
    decimal TotalAccountBalances,
    decimal MonthlyIncome,
    decimal MonthlyExpenses,
    decimal NetCashFlow,
    decimal OverallBudgetUtilization,
    int Year,
    int Month,
    IReadOnlyList<SavingsProgressDto> SavingsProgress);

/// <summary>
/// Income/expense/net cash flow totals for a single calendar month, used to build trend charts.
/// </summary>
public sealed record MonthlyTrendDto(int Year, int Month, decimal Income, decimal Expenses, decimal NetCashFlow);

/// <summary>
/// Total expenses attributed to a single category within the requested date range.
/// </summary>
public sealed record CategoryExpenseDto(Guid TransactionCategoryId, string CategoryName, decimal TotalExpense);

/// <summary>
/// Budget allocation vs. actual spending for a single category, for the budget overview widget.
/// </summary>
public sealed record BudgetOverviewItemDto(
    Guid TransactionCategoryId,
    string CategoryName,
    decimal AllocatedBudget,
    decimal ActualExpenses,
    decimal RemainingBudget,
    decimal BudgetUtilization);

/// <summary>
/// A bill due soon, surfaced on the dashboard as a reminder.
/// </summary>
public sealed record UpcomingBillDto(Guid Id, string Name, decimal Amount, DateTimeOffset DueDateUtc, bool IsOverdue);

/// <summary>
/// Progress of a single savings goal toward its target amount.
/// </summary>
public sealed record SavingsProgressDto(Guid Id, string Name, decimal TargetAmount, decimal CurrentAmount, decimal ProgressPercentage);
