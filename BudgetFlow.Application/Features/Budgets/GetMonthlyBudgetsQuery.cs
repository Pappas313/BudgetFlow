using BudgetFlow.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BudgetFlow.Application.Features.Budgets;

/// <summary>
/// Query to retrieve the monthly budget summary (allocations vs. actual spending) for the
/// requested year and month, defaulting to the current month.
/// </summary>
public sealed record GetMonthlyBudgetsQuery(int? Year, int? Month) : IRequest<BudgetSummaryDto>;

public sealed class GetMonthlyBudgetsQueryHandler : IRequestHandler<GetMonthlyBudgetsQuery, BudgetSummaryDto>
{
    private readonly IBudgetFlowDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetMonthlyBudgetsQueryHandler(IBudgetFlowDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<BudgetSummaryDto> Handle(GetMonthlyBudgetsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var year = request.Year ?? now.Year;
        var month = request.Month ?? now.Month;

        var userId = _currentUserService.UserId;
        if (userId is null)
        {
            return new BudgetSummaryDto(year, month, 0, 0, 0, 0, Array.Empty<MonthlyBudgetDto>());
        }

        var budgets = await _dbContext.MonthlyBudgets
            .AsNoTracking()
            .Include(b => b.TransactionCategory)
            .Where(b => b.ApplicationUserId == userId.Value && b.Year == year && b.Month == month)
            .OrderBy(b => b.TransactionCategory!.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var budgetDtos = new List<MonthlyBudgetDto>(budgets.Count);
        foreach (var budget in budgets)
        {
            var actualExpenses = await BudgetActualsCalculator
                .GetActualExpensesAsync(_dbContext, userId.Value, budget.TransactionCategoryId, year, month, cancellationToken)
                .ConfigureAwait(false);

            budgetDtos.Add(BudgetMapper.ToDto(budget, actualExpenses));
        }

        var totalAllocated = budgetDtos.Sum(b => b.AllocatedBudget);
        var totalActual = budgetDtos.Sum(b => b.ActualExpenses);
        var overallUtilization = totalAllocated == 0 ? 0 : totalActual / totalAllocated * 100m;

        return new BudgetSummaryDto(
            year,
            month,
            totalAllocated,
            totalActual,
            totalAllocated - totalActual,
            overallUtilization,
            budgetDtos);
    }
}
