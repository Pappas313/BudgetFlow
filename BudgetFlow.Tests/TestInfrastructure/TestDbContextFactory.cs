using BudgetFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BudgetFlow.Tests.TestInfrastructure;

internal static class TestDbContextFactory
{
    public static BudgetFlowDbContext Create()
    {
        var options = new DbContextOptionsBuilder<BudgetFlowDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new BudgetFlowDbContext(options);
    }
}
