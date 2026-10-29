namespace BudgetFlow.Application.Abstractions;

/// <summary>
/// Exposes information about the currently authenticated user for the duration
/// of a single request, enabling resource queries to be scoped to the owner.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// The unique identifier of the authenticated user, or <c>null</c> if the
    /// current request is not authenticated.
    /// </summary>
    Guid? UserId { get; }

    bool IsAuthenticated { get; }
}
