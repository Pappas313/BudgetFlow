using BudgetFlow.Application.Features.Dashboard;
using BudgetFlow.Domain.Entities;
using BudgetFlow.Domain.Enums;
using BudgetFlow.Tests.TestInfrastructure;
using Xunit;

namespace BudgetFlow.Tests.Dashboard;

public sealed class GetDashboardSummaryQueryHandlerTests
{
    [Fact]
    public async Task Handle_ComputesNetCashFlow_ExcludingTransfers()
    {
        await using var dbContext = TestDbContextFactory.Create();

        var userId = Guid.NewGuid();
        var account = new BankAccount(userId, "Checking", "USD", openingBalance: 1000m);
        var otherAccount = new BankAccount(userId, "Savings", "USD");
        dbContext.BankAccounts.AddRange(account, otherAccount);
        await dbContext.SaveChangesAsync();

        var now = DateTimeOffset.UtcNow;
        var periodStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);

        var transferGroupId = Guid.NewGuid();
        dbContext.FinancialTransactions.AddRange(
            new FinancialTransaction(account.Id, TransactionType.Income, 500m, periodStart.AddDays(2)),
            new FinancialTransaction(account.Id, TransactionType.Expense, -200m, periodStart.AddDays(3)),
            new FinancialTransaction(account.Id, TransactionType.Transfer, -300m, periodStart.AddDays(4), transferGroupId: transferGroupId),
            new FinancialTransaction(otherAccount.Id, TransactionType.Transfer, 300m, periodStart.AddDays(4), transferGroupId: transferGroupId));
        await dbContext.SaveChangesAsync();

        var handler = new GetDashboardSummaryQueryHandler(dbContext, new FakeCurrentUserService(userId));

        var result = await handler.Handle(new GetDashboardSummaryQuery(now.Year, now.Month), CancellationToken.None);

        Assert.Equal(500m, result.MonthlyIncome);
        Assert.Equal(200m, result.MonthlyExpenses);
        Assert.Equal(300m, result.NetCashFlow);
    }

    [Fact]
    public async Task Handle_ReturnsZeroedSummary_WhenNotAuthenticated()
    {
        await using var dbContext = TestDbContextFactory.Create();

        var handler = new GetDashboardSummaryQueryHandler(dbContext, new FakeCurrentUserService(null));

        var result = await handler.Handle(new GetDashboardSummaryQuery(2025, 1), CancellationToken.None);

        Assert.Equal(0m, result.TotalAccountBalances);
        Assert.Equal(0m, result.MonthlyIncome);
        Assert.Equal(0m, result.MonthlyExpenses);
        Assert.Empty(result.SavingsProgress);
    }
}

public sealed class GetMonthlyTrendsQueryHandlerTests
{
    [Fact]
    public async Task Handle_BucketsTransactionsByCalendarMonth_ExcludingTransfers()
    {
        await using var dbContext = TestDbContextFactory.Create();

        var userId = Guid.NewGuid();
        var account = new BankAccount(userId, "Checking", "USD");
        dbContext.BankAccounts.Add(account);
        await dbContext.SaveChangesAsync();

        var now = DateTimeOffset.UtcNow;
        var thisMonthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var lastMonthStart = thisMonthStart.AddMonths(-1);

        dbContext.FinancialTransactions.AddRange(
            new FinancialTransaction(account.Id, TransactionType.Income, 1000m, thisMonthStart.AddDays(1)),
            new FinancialTransaction(account.Id, TransactionType.Expense, -400m, thisMonthStart.AddDays(2)),
            new FinancialTransaction(account.Id, TransactionType.Income, 800m, lastMonthStart.AddDays(1)),
            new FinancialTransaction(account.Id, TransactionType.Expense, -300m, lastMonthStart.AddDays(2)),
            new FinancialTransaction(account.Id, TransactionType.Transfer, -100m, thisMonthStart.AddDays(3)));
        await dbContext.SaveChangesAsync();

        var handler = new GetMonthlyTrendsQueryHandler(dbContext, new FakeCurrentUserService(userId));

        var result = await handler.Handle(new GetMonthlyTrendsQuery(MonthsBack: 2), CancellationToken.None);

        Assert.Equal(2, result.Count);
        var thisMonth = result[^1];
        var lastMonth = result[^2];

        Assert.Equal(1000m, thisMonth.Income);
        Assert.Equal(400m, thisMonth.Expenses);
        Assert.Equal(600m, thisMonth.NetCashFlow);

        Assert.Equal(800m, lastMonth.Income);
        Assert.Equal(300m, lastMonth.Expenses);
        Assert.Equal(500m, lastMonth.NetCashFlow);
    }
}
