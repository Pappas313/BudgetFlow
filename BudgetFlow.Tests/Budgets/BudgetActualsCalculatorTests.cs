using BudgetFlow.Application.Abstractions;
using BudgetFlow.Application.Features.Budgets;
using BudgetFlow.Domain.Entities;
using BudgetFlow.Domain.Enums;
using BudgetFlow.Tests.TestInfrastructure;
using Xunit;

namespace BudgetFlow.Tests.Budgets;

public sealed class BudgetActualsCalculatorTests
{
    [Fact]
    public async Task GetActualExpensesAsync_SumsExpensesForCategoryAndMonth_ExcludingOtherMonthsAndUsers()
    {
        await using var dbContext = TestDbContextFactory.Create();

        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var account = new BankAccount(userId, "Checking", "USD");
        var otherAccount = new BankAccount(otherUserId, "Checking", "USD");
        dbContext.BankAccounts.AddRange(account, otherAccount);
        await dbContext.SaveChangesAsync();

        dbContext.FinancialTransactions.AddRange(
            new FinancialTransaction(account.Id, TransactionType.Expense, -100m, new DateTimeOffset(2025, 3, 10, 0, 0, 0, TimeSpan.Zero), categoryId),
            new FinancialTransaction(account.Id, TransactionType.Expense, -50m, new DateTimeOffset(2025, 3, 20, 0, 0, 0, TimeSpan.Zero), categoryId),
            new FinancialTransaction(account.Id, TransactionType.Expense, -75m, new DateTimeOffset(2025, 4, 1, 0, 0, 0, TimeSpan.Zero), categoryId),
            new FinancialTransaction(otherAccount.Id, TransactionType.Expense, -999m, new DateTimeOffset(2025, 3, 15, 0, 0, 0, TimeSpan.Zero), categoryId));

        await dbContext.SaveChangesAsync();

        var actual = await BudgetActualsCalculator.GetActualExpensesAsync(
            dbContext, userId, categoryId, 2025, 3, CancellationToken.None);

        Assert.Equal(150m, actual);
    }
}
