namespace BudgetFlow.Application.Features.Authentication;

/// <summary>
/// Request payload for registering a new BudgetFlow user.
/// </summary>
public sealed record RegisterRequest(string Email, string Password, string DisplayName);

/// <summary>
/// Request payload for authenticating an existing BudgetFlow user.
/// </summary>
public sealed record LoginRequest(string Email, string Password);

/// <summary>
/// Request payload for exchanging a refresh token for a new access/refresh token pair.
/// </summary>
public sealed record RefreshRequest(string RefreshToken);

/// <summary>
/// Request payload for revoking a refresh token (logout).
/// </summary>
public sealed record LogoutRequest(string RefreshToken);

/// <summary>
/// Response payload containing an access token and refresh token pair.
/// </summary>
public sealed record AuthTokenResponse(string AccessToken, DateTimeOffset AccessTokenExpiresAtUtc, string RefreshToken, DateTimeOffset RefreshTokenExpiresAtUtc);

/// <summary>
/// Response payload describing the currently authenticated user.
/// </summary>
public sealed record CurrentUserResponse(Guid Id, string Email, string DisplayName);
