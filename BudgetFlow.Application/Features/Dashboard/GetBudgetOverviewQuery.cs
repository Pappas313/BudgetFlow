using BudgetFlow.Application.Abstractions;
using BudgetFlow.Application.Features.Budgets;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BudgetFlow.Application.Features.Dashboard;

/// <summary>
/// Query to compute budget allocation vs. actual spending per category for the current user for
/// a given calendar month (defaults to the current month).
/// </summary>
public sealed record GetBudgetOverviewQuery(int? Year = null, int? Month = null) : IRequest<IReadOnlyList<BudgetOverviewItemDto>>;

public sealed class GetBudgetOverviewQueryHandler : IRequestHandler<GetBudgetOverviewQuery, IReadOnlyList<BudgetOverviewItemDto>>
{
    private readonly IBudgetFlowDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetBudgetOverviewQueryHandler(IBudgetFlowDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<BudgetOverviewItemDto>> Handle(GetBudgetOverviewQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
        {
            return Array.Empty<BudgetOverviewItemDto>();
        }

        var utcNow = DateTimeOffset.UtcNow;
        var year = request.Year ?? utcNow.Year;
        var month = request.Month ?? utcNow.Month;

        var budgets = await _dbContext.MonthlyBudgets
            .AsNoTracking()
            .Include(b => b.TransactionCategory)
            .Where(b => b.ApplicationUserId == userId.Value && b.Year == year && b.Month == month)
            .OrderBy(b => b.TransactionCategory!.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var items = new List<BudgetOverviewItemDto>(budgets.Count);
        foreach (var budget in budgets)
        {
            var actualExpenses = await BudgetActualsCalculator
                .GetActualExpensesAsync(_dbContext, userId.Value, budget.TransactionCategoryId, year, month, cancellationToken)
                .ConfigureAwait(false);

            var remaining = budget.BudgetedAmount - actualExpenses;
            var utilization = budget.BudgetedAmount == 0 ? 0 : actualExpenses / budget.BudgetedAmount * 100m;

            items.Add(new BudgetOverviewItemDto(
                budget.TransactionCategoryId,
                budget.TransactionCategory?.Name ?? string.Empty,
                budget.BudgetedAmount,
                actualExpenses,
                remaining,
                utilization));
        }

        return items;
    }
}
