using BudgetFlow.Application.Features.Budgets;
using BudgetFlow.Domain.Entities;
using Xunit;

namespace BudgetFlow.Tests.Budgets;

public sealed class BudgetMapperTests
{
    [Fact]
    public void ToDto_ComputesRemainingBudget_AsAllocatedMinusActual()
    {
        var budget = new MonthlyBudget(Guid.NewGuid(), Guid.NewGuid(), 2025, 1, 500m);

        var dto = BudgetMapper.ToDto(budget, actualExpenses: 300m);

        Assert.Equal(200m, dto.RemainingBudget);
    }

    [Fact]
    public void ToDto_ComputesBudgetUtilization_AsPercentageOfAllocated()
    {
        var budget = new MonthlyBudget(Guid.NewGuid(), Guid.NewGuid(), 2025, 1, 400m);

        var dto = BudgetMapper.ToDto(budget, actualExpenses: 100m);

        Assert.Equal(25m, dto.BudgetUtilization);
    }

    [Fact]
    public void ToDto_ComputesBudgetVariance_AsAllocatedMinusActual()
    {
        var budget = new MonthlyBudget(Guid.NewGuid(), Guid.NewGuid(), 2025, 1, 500m);

        var dto = BudgetMapper.ToDto(budget, actualExpenses: 600m);

        Assert.Equal(-100m, dto.BudgetVariance);
    }

    [Fact]
    public void ToDto_ReturnsZeroUtilization_WhenAllocatedBudgetIsZero()
    {
        var budget = new MonthlyBudget(Guid.NewGuid(), Guid.NewGuid(), 2025, 1, 0m);

        var dto = BudgetMapper.ToDto(budget, actualExpenses: 50m);

        Assert.Equal(0m, dto.BudgetUtilization);
    }

    [Fact]
    public void ToDto_OverspentBudget_ProducesNegativeRemainingAndOverUtilization()
    {
        var budget = new MonthlyBudget(Guid.NewGuid(), Guid.NewGuid(), 2025, 1, 200m);

        var dto = BudgetMapper.ToDto(budget, actualExpenses: 250m);

        Assert.Equal(-50m, dto.RemainingBudget);
        Assert.Equal(125m, dto.BudgetUtilization);
    }
}
