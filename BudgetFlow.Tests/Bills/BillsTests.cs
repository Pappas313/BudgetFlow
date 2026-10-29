using BudgetFlow.Application.Exceptions;
using BudgetFlow.Application.Features.Bills;
using BudgetFlow.Domain.Entities;
using BudgetFlow.Tests.TestInfrastructure;
using Xunit;

namespace BudgetFlow.Tests.Bills;

public sealed class BillsTests
{
    [Fact]
    public async Task GetBillsQuery_ReturnsOnlyBillsOwnedByCurrentUser()
    {
        await using var dbContext = TestDbContextFactory.Create();

        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        dbContext.Bills.Add(new Bill(ownerId, "Electricity", 100m, DateTimeOffset.UtcNow.AddDays(5)));
        dbContext.Bills.Add(new Bill(otherUserId, "Water", 50m, DateTimeOffset.UtcNow.AddDays(5)));
        await dbContext.SaveChangesAsync();

        var handler = new GetBillsQueryHandler(dbContext, new FakeCurrentUserService(ownerId));

        var result = await handler.Handle(new GetBillsQuery(), CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("Electricity", result[0].Name);
    }

    [Fact]
    public async Task GetOverdueBillsQuery_ReturnsBillsPastDueWithoutPayment()
    {
        await using var dbContext = TestDbContextFactory.Create();

        var userId = Guid.NewGuid();
        var overdueBill = new Bill(userId, "Rent", 1000m, DateTimeOffset.UtcNow.AddDays(-3));
        var paidBill = new Bill(userId, "Internet", 60m, DateTimeOffset.UtcNow.AddDays(-2));
        var futureBill = new Bill(userId, "Insurance", 200m, DateTimeOffset.UtcNow.AddDays(10));

        dbContext.Bills.AddRange(overdueBill, paidBill, futureBill);
        await dbContext.SaveChangesAsync();

        dbContext.BillPayments.Add(new BillPayment(paidBill.Id, 60m, DateTimeOffset.UtcNow.AddDays(-1)));
        await dbContext.SaveChangesAsync();

        var handler = new GetOverdueBillsQueryHandler(dbContext, new FakeCurrentUserService(userId));

        var result = await handler.Handle(new GetOverdueBillsQuery(), CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("Rent", result[0].Name);
        Assert.True(result[0].IsOverdue);
    }

    [Fact]
    public async Task GetUpcomingBillsQuery_ReturnsUnpaidBillsWithinWindow()
    {
        await using var dbContext = TestDbContextFactory.Create();

        var userId = Guid.NewGuid();
        var upcomingBill = new Bill(userId, "Phone", 40m, DateTimeOffset.UtcNow.AddDays(3));
        var farBill = new Bill(userId, "Car Insurance", 300m, DateTimeOffset.UtcNow.AddDays(60));

        dbContext.Bills.AddRange(upcomingBill, farBill);
        await dbContext.SaveChangesAsync();

        var handler = new GetUpcomingBillsQueryHandler(dbContext, new FakeCurrentUserService(userId));

        var result = await handler.Handle(new GetUpcomingBillsQuery(WithinDays: 7), CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("Phone", result[0].Name);
    }

    [Fact]
    public async Task UpdateBillCommand_ThrowsForbidden_WhenNotOwner()
    {
        await using var dbContext = TestDbContextFactory.Create();

        var ownerId = Guid.NewGuid();
        var attackerId = Guid.NewGuid();

        var bill = new Bill(ownerId, "Rent", 1000m, DateTimeOffset.UtcNow.AddDays(5));
        dbContext.Bills.Add(bill);
        await dbContext.SaveChangesAsync();

        var handler = new UpdateBillCommandHandler(dbContext, new FakeCurrentUserService(attackerId));

        var request = new UpdateBillRequest("Rent", 1200m, DateTimeOffset.UtcNow.AddDays(5), false);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(new UpdateBillCommand(bill.Id, request), CancellationToken.None));
    }

    [Fact]
    public async Task RecordBillPaymentCommand_AddsPaymentToHistory()
    {
        await using var dbContext = TestDbContextFactory.Create();

        var userId = Guid.NewGuid();
        var bill = new Bill(userId, "Internet", 60m, DateTimeOffset.UtcNow.AddDays(1));
        dbContext.Bills.Add(bill);
        await dbContext.SaveChangesAsync();

        var handler = new RecordBillPaymentCommandHandler(dbContext, new FakeCurrentUserService(userId));
        var request = new RecordBillPaymentRequest(60m, DateTimeOffset.UtcNow);

        var result = await handler.Handle(new RecordBillPaymentCommand(bill.Id, request), CancellationToken.None);

        Assert.Equal(bill.Id, result.BillId);
        Assert.Equal(60m, result.AmountPaid);
    }
}
