namespace BudgetFlow.Application.Features.Authentication;

/// <summary>
/// Represents the outcome of an authentication operation, allowing the caller
/// to distinguish between validation/credential failures and success without
/// relying on exceptions for expected control flow.
/// </summary>
public sealed class AuthResult<T>
{
    public bool Succeeded { get; }

    public T? Value { get; }

    public IReadOnlyCollection<string> Errors { get; }

    private AuthResult(bool succeeded, T? value, IReadOnlyCollection<string> errors)
    {
        Succeeded = succeeded;
        Value = value;
        Errors = errors;
    }

    public static AuthResult<T> Success(T value) => new(true, value, Array.Empty<string>());

    public static AuthResult<T> Failure(params string[] errors) => new(false, default, errors);

    public static AuthResult<T> Failure(IEnumerable<string> errors) => new(false, default, errors.ToArray());
}
