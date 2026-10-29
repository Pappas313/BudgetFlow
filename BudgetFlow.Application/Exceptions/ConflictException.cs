namespace BudgetFlow.Application.Exceptions;

/// <summary>
/// Thrown when a request conflicts with the current state of a resource (e.g. a duplicate
/// imported transaction), resulting in an HTTP 409 response.
/// </summary>
public sealed class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }
}
