using BudgetFlow.Application.Abstractions;
using BudgetFlow.Application.Features.Budgets;
using BudgetFlow.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BudgetFlow.Application.Features.Dashboard;

/// <summary>
/// Query to compute the high-level financial dashboard summary for the current user for a given
/// calendar month (defaults to the current month).
/// </summary>
public sealed record GetDashboardSummaryQuery(int? Year = null, int? Month = null) : IRequest<DashboardSummaryDto>;

public sealed class GetDashboardSummaryQueryHandler : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryDto>
{
    private readonly IBudgetFlowDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetDashboardSummaryQueryHandler(IBudgetFlowDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<DashboardSummaryDto> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        var utcNow = DateTimeOffset.UtcNow;
        var year = request.Year ?? utcNow.Year;
        var month = request.Month ?? utcNow.Month;

        var userId = _currentUserService.UserId;
        if (userId is null)
        {
            return new DashboardSummaryDto(0, 0, 0, 0, 0, year, month, Array.Empty<SavingsProgressDto>());
        }

        var periodStart = new DateTimeOffset(year, month, 1, 0, 0, 0, TimeSpan.Zero);
        var periodEnd = periodStart.AddMonths(1);

        // Total account balances across all non-archived accounts owned by the user. The sum
        // intentionally does not perform currency conversion; callers are expected to filter or
        // interpret results per currency when accounts span multiple currencies.
        var totalBalances = await _dbContext.BankAccounts
            .AsNoTracking()
            .Where(a => a.ApplicationUserId == userId.Value)
            .SumAsync(a => (decimal?)a.CurrentBalance, cancellationToken)
            .ConfigureAwait(false) ?? 0m;

        // Transfers are excluded: they move funds between the user's own accounts and do not
        // represent real income or expense.
        var monthlyTransactions = _dbContext.FinancialTransactions
            .AsNoTracking()
            .Where(t => t.BankAccount!.ApplicationUserId == userId.Value
                && t.OccurredAtUtc >= periodStart
                && t.OccurredAtUtc < periodEnd);

        var monthlyIncome = await monthlyTransactions
            .Where(t => t.TransactionType == TransactionType.Income)
            .SumAsync(t => (decimal?)t.Amount, cancellationToken)
            .ConfigureAwait(false) ?? 0m;

        var monthlyExpenseMagnitude = await monthlyTransactions
            .Where(t => t.TransactionType == TransactionType.Expense)
            .SumAsync(t => (decimal?)t.Amount, cancellationToken)
            .ConfigureAwait(false) ?? 0m;

        var monthlyExpenses = Math.Abs(monthlyExpenseMagnitude);
        var netCashFlow = monthlyIncome - monthlyExpenses;

        var budgets = await _dbContext.MonthlyBudgets
            .AsNoTracking()
            .Where(b => b.ApplicationUserId == userId.Value && b.Year == year && b.Month == month)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        decimal overallUtilization = 0;
        if (budgets.Count > 0)
        {
            var totalAllocated = budgets.Sum(b => b.BudgetedAmount);
            if (totalAllocated > 0)
            {
                decimal totalActual = 0;
                foreach (var budget in budgets)
                {
                    totalActual += await BudgetActualsCalculator
                        .GetActualExpensesAsync(_dbContext, userId.Value, budget.TransactionCategoryId, year, month, cancellationToken)
                        .ConfigureAwait(false);
                }

                overallUtilization = totalActual / totalAllocated * 100m;
            }
        }

        var savingsGoals = await _dbContext.SavingsGoals
            .AsNoTracking()
            .Where(g => g.ApplicationUserId == userId.Value)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var savingsProgress = savingsGoals
            .Select(g => new SavingsProgressDto(
                g.Id,
                g.Name,
                g.TargetAmount,
                g.CurrentAmount,
                g.TargetAmount == 0 ? 0 : g.CurrentAmount / g.TargetAmount * 100m))
            .ToList();

        return new DashboardSummaryDto(
            totalBalances,
            monthlyIncome,
            monthlyExpenses,
            netCashFlow,
            overallUtilization,
            year,
            month,
            savingsProgress);
    }
}
