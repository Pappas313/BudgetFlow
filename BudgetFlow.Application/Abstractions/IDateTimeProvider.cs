namespace BudgetFlow.Application.Abstractions;

/// <summary>
/// Provides the current date and time, abstracted so that Application layer
/// code remains testable and does not depend directly on <see cref="System.DateTimeOffset"/>.UtcNow
/// or <see cref="System.DateOnly"/>.FromDateTime(DateTime.UtcNow).
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>
    /// Gets the current UTC date and time.
    /// </summary>
    DateTimeOffset UtcNow { get; }

    /// <summary>
    /// Gets the current UTC calendar date.
    /// </summary>
    DateOnly Today { get; }
}
