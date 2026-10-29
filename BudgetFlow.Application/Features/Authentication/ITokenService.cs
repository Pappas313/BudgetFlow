namespace BudgetFlow.Application.Features.Authentication;

/// <summary>
/// Describes the identity and claims required to issue a JWT access token.
/// </summary>
public sealed record TokenSubject(Guid UserId, string Email, IReadOnlyCollection<string> Roles);

/// <summary>
/// Generates and validates JWT access tokens and opaque refresh tokens.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Creates a signed JWT access token for the given subject.
    /// </summary>
    (string AccessToken, DateTimeOffset ExpiresAtUtc) CreateAccessToken(TokenSubject subject);

    /// <summary>
    /// Creates a new cryptographically random refresh token value (raw, unhashed).
    /// </summary>
    string CreateRefreshTokenValue();

    /// <summary>
    /// Computes a stable hash for a raw refresh token value, suitable for persistence and lookup.
    /// </summary>
    string HashRefreshToken(string rawToken);

    /// <summary>
    /// The configured refresh token lifetime.
    /// </summary>
    TimeSpan RefreshTokenLifetime { get; }
}
