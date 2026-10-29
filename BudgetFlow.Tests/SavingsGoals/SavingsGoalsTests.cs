using BudgetFlow.Application.Exceptions;
using BudgetFlow.Application.Features.SavingsGoals;
using BudgetFlow.Domain.Entities;
using BudgetFlow.Tests.TestInfrastructure;
using Xunit;

namespace BudgetFlow.Tests.SavingsGoals;

public sealed class SavingsGoalsTests
{
    [Fact]
    public async Task CreateSavingsGoalCommand_CreatesGoalOwnedByCurrentUser()
    {
        await using var dbContext = TestDbContextFactory.Create();

        var userId = Guid.NewGuid();
        var handler = new CreateSavingsGoalCommandHandler(dbContext, new FakeCurrentUserService(userId));
        var request = new CreateSavingsGoalRequest("Emergency Fund", 5000m, DateTimeOffset.UtcNow.AddMonths(12));

        var result = await handler.Handle(new CreateSavingsGoalCommand(request), CancellationToken.None);

        Assert.Equal("Emergency Fund", result.Name);
        Assert.Equal(5000m, result.TargetAmount);
        Assert.Equal(0m, result.CurrentAmount);

        var stored = Assert.Single(dbContext.SavingsGoals);
        Assert.Equal(userId, stored.ApplicationUserId);
    }

    [Fact]
    public async Task CreateSavingsGoalCommand_ThrowsForbidden_WhenNotAuthenticated()
    {
        await using var dbContext = TestDbContextFactory.Create();

        var handler = new CreateSavingsGoalCommandHandler(dbContext, new FakeCurrentUserService(null));
        var request = new CreateSavingsGoalRequest("Emergency Fund", 5000m, null);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(new CreateSavingsGoalCommand(request), CancellationToken.None));
    }

    [Fact]
    public async Task RecordSavingsContributionCommand_IncreasesCurrentAmount()
    {
        await using var dbContext = TestDbContextFactory.Create();

        var userId = Guid.NewGuid();
        var goal = new SavingsGoal(userId, "Vacation", 2000m, null);
        dbContext.SavingsGoals.Add(goal);
        await dbContext.SaveChangesAsync();

        var handler = new RecordSavingsContributionCommandHandler(dbContext, new FakeCurrentUserService(userId));
        var request = new RecordSavingsContributionRequest(250m, DateTimeOffset.UtcNow);

        var result = await handler.Handle(
            new RecordSavingsContributionCommand(goal.Id, request), CancellationToken.None);

        Assert.Equal(250m, result.Amount);
        Assert.Equal(250m, goal.CurrentAmount);

        var storedContribution = Assert.Single(dbContext.SavingsContributions);
        Assert.Equal(goal.Id, storedContribution.SavingsGoalId);
    }

    [Fact]
    public async Task RecordSavingsContributionCommand_ThrowsForbidden_WhenNotOwner()
    {
        await using var dbContext = TestDbContextFactory.Create();

        var ownerId = Guid.NewGuid();
        var attackerId = Guid.NewGuid();
        var goal = new SavingsGoal(ownerId, "Vacation", 2000m, null);
        dbContext.SavingsGoals.Add(goal);
        await dbContext.SaveChangesAsync();

        var handler = new RecordSavingsContributionCommandHandler(dbContext, new FakeCurrentUserService(attackerId));
        var request = new RecordSavingsContributionRequest(100m, DateTimeOffset.UtcNow);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(new RecordSavingsContributionCommand(goal.Id, request), CancellationToken.None));
    }

    [Fact]
    public async Task RecordSavingsContributionCommand_ThrowsNotFound_WhenGoalDoesNotExist()
    {
        await using var dbContext = TestDbContextFactory.Create();

        var handler = new RecordSavingsContributionCommandHandler(
            dbContext, new FakeCurrentUserService(Guid.NewGuid()));
        var request = new RecordSavingsContributionRequest(100m, DateTimeOffset.UtcNow);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new RecordSavingsContributionCommand(Guid.NewGuid(), request), CancellationToken.None));
    }
}
