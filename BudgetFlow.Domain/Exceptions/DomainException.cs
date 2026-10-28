namespace BudgetFlow.Domain.Exceptions;

/// <summary>
/// Base type for all exceptions raised due to a violation of a domain
/// invariant or business rule within the BudgetFlow domain.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message)
    {
    }

    protected DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
