namespace BudgetFlow.Application.Exceptions;

/// <summary>
/// Thrown when the current user attempts to access or modify a resource they do not own,
/// resulting in an HTTP 403 response.
/// </summary>
public sealed class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException()
        : base("You do not have permission to access this resource.")
    {
    }

    public ForbiddenAccessException(string message) : base(message)
    {
    }
}
