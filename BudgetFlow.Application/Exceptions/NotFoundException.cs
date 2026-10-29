namespace BudgetFlow.Application.Exceptions;

/// <summary>
/// Thrown when a requested resource cannot be located, resulting in an HTTP 404 response.
/// </summary>
public sealed class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string entityName, object key)
        : base($"Entity \"{entityName}\" ({key}) was not found.")
    {
    }
}
