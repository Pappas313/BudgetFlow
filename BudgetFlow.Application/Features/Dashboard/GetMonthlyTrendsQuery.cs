using BudgetFlow.Application.Abstractions;
using BudgetFlow.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BudgetFlow.Application.Features.Dashboard;

/// <summary>
/// Query to compute income/expense/net cash flow trends for the current user over the trailing
/// twelve calendar months (including the current month), excluding transfers.
/// </summary>
public sealed record GetMonthlyTrendsQuery(int MonthsBack = 12) : IRequest<IReadOnlyList<MonthlyTrendDto>>;

public sealed class GetMonthlyTrendsQueryHandler : IRequestHandler<GetMonthlyTrendsQuery, IReadOnlyList<MonthlyTrendDto>>
{
    private readonly IBudgetFlowDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetMonthlyTrendsQueryHandler(IBudgetFlowDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<MonthlyTrendDto>> Handle(GetMonthlyTrendsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
        {
            return Array.Empty<MonthlyTrendDto>();
        }

        var utcNow = DateTimeOffset.UtcNow;
        var currentPeriodStart = new DateTimeOffset(utcNow.Year, utcNow.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var rangeStart = currentPeriodStart.AddMonths(-(request.MonthsBack - 1));
        var rangeEnd = currentPeriodStart.AddMonths(1);

        // Transfers are excluded: they move funds between the user's own accounts and do not
        // represent real income or expense.
        var transactions = await _dbContext.FinancialTransactions
            .AsNoTracking()
            .Where(t => t.BankAccount!.ApplicationUserId == userId.Value
                && t.OccurredAtUtc >= rangeStart
                && t.OccurredAtUtc < rangeEnd
                && t.TransactionType != TransactionType.Transfer)
            .Select(t => new { t.TransactionType, t.Amount, t.OccurredAtUtc })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var trends = new List<MonthlyTrendDto>(request.MonthsBack);
        for (var i = 0; i < request.MonthsBack; i++)
        {
            var periodStart = rangeStart.AddMonths(i);
            var periodEnd = periodStart.AddMonths(1);

            var periodTransactions = transactions
                .Where(t => t.OccurredAtUtc >= periodStart && t.OccurredAtUtc < periodEnd)
                .ToList();

            var income = periodTransactions
                .Where(t => t.TransactionType == TransactionType.Income)
                .Sum(t => t.Amount);

            var expenses = Math.Abs(periodTransactions
                .Where(t => t.TransactionType == TransactionType.Expense)
                .Sum(t => t.Amount));

            trends.Add(new MonthlyTrendDto(periodStart.Year, periodStart.Month, income, expenses, income - expenses));
        }

        return trends;
    }
}
