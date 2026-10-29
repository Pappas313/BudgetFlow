using BudgetFlow.Application.Abstractions;

namespace BudgetFlow.Tests.TestInfrastructure;

internal sealed class FakeDateTimeProvider : IDateTimeProvider
{
    public FakeDateTimeProvider(DateTimeOffset utcNow)
    {
        UtcNow = utcNow;
    }

    public DateTimeOffset UtcNow { get; }

    public DateOnly Today => DateOnly.FromDateTime(UtcNow.UtcDateTime);
}
