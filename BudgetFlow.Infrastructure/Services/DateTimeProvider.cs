using BudgetFlow.Application.Abstractions;

namespace BudgetFlow.Infrastructure.Services;

/// <summary>
/// System-clock based implementation of <see cref="IDateTimeProvider"/>.
/// </summary>
public sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;

    public DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);
}
