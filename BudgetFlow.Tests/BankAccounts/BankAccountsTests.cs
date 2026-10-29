using BudgetFlow.Application.Exceptions;
using BudgetFlow.Application.Features.BankAccounts;
using BudgetFlow.Domain.Entities;
using BudgetFlow.Domain.Enums;
using BudgetFlow.Tests.TestInfrastructure;
using Xunit;

namespace BudgetFlow.Tests.BankAccounts;

public sealed class BankAccountsTests
{
    [Fact]
    public async Task CreateBankAccountCommand_CreatesAccountOwnedByCurrentUser()
    {
        await using var dbContext = TestDbContextFactory.Create();

        var userId = Guid.NewGuid();
        var handler = new CreateBankAccountCommandHandler(dbContext, new FakeCurrentUserService(userId));
        var request = new CreateBankAccountRequest("Checking", "usd", AccountType.Checking, "Acme Bank", 500m);

        var result = await handler.Handle(new CreateBankAccountCommand(request), CancellationToken.None);

        Assert.Equal("Checking", result.Name);
        Assert.Equal("USD", result.Currency);
        Assert.Equal(500m, result.CurrentBalance);

        var stored = Assert.Single(dbContext.BankAccounts);
        Assert.Equal(userId, stored.ApplicationUserId);
    }

    [Fact]
    public async Task CreateBankAccountCommand_ThrowsForbidden_WhenNotAuthenticated()
    {
        await using var dbContext = TestDbContextFactory.Create();

        var handler = new CreateBankAccountCommandHandler(dbContext, new FakeCurrentUserService(null));
        var request = new CreateBankAccountRequest("Checking", "USD", AccountType.Checking, null, 0m);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(new CreateBankAccountCommand(request), CancellationToken.None));
    }

    [Fact]
    public async Task GetAccountBalanceQuery_ReturnsOpeningBalancePlusTransactions()
    {
        await using var dbContext = TestDbContextFactory.Create();

        var userId = Guid.NewGuid();
        var account = new BankAccount(userId, "Checking", "USD", AccountType.Checking, "Acme Bank", 1000m);
        dbContext.BankAccounts.Add(account);
        await dbContext.SaveChangesAsync();

        dbContext.FinancialTransactions.Add(new FinancialTransaction(
            account.Id, TransactionType.Income, 200m, DateTimeOffset.UtcNow));
        dbContext.FinancialTransactions.Add(new FinancialTransaction(
            account.Id, TransactionType.Expense, -50m, DateTimeOffset.UtcNow));
        await dbContext.SaveChangesAsync();

        var fixedNow = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var handler = new GetAccountBalanceQueryHandler(
            dbContext, new FakeCurrentUserService(userId), new FakeDateTimeProvider(fixedNow));

        var result = await handler.Handle(new GetAccountBalanceQuery(account.Id), CancellationToken.None);

        Assert.Equal(1150m, result.Balance);
        Assert.Equal("USD", result.Currency);
        Assert.Equal(fixedNow, result.AsOfUtc);
    }

    [Fact]
    public async Task GetAccountBalanceQuery_ThrowsForbidden_WhenAccountOwnedByAnotherUser()
    {
        await using var dbContext = TestDbContextFactory.Create();

        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var account = new BankAccount(ownerId, "Checking", "USD", AccountType.Checking, null, 100m);
        dbContext.BankAccounts.Add(account);
        await dbContext.SaveChangesAsync();

        var handler = new GetAccountBalanceQueryHandler(
            dbContext, new FakeCurrentUserService(otherUserId), new FakeDateTimeProvider(DateTimeOffset.UtcNow));

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(new GetAccountBalanceQuery(account.Id), CancellationToken.None));
    }

    [Fact]
    public async Task GetAccountBalanceQuery_ThrowsNotFound_WhenAccountDoesNotExist()
    {
        await using var dbContext = TestDbContextFactory.Create();

        var handler = new GetAccountBalanceQueryHandler(
            dbContext, new FakeCurrentUserService(Guid.NewGuid()), new FakeDateTimeProvider(DateTimeOffset.UtcNow));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new GetAccountBalanceQuery(Guid.NewGuid()), CancellationToken.None));
    }
}
