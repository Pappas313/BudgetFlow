namespace BudgetFlow.Application.Features.Authentication;

/// <summary>
/// Orchestrates user registration, login, refresh-token rotation, and logout.
/// </summary>
public interface IAuthService
{
    Task<AuthResult<AuthTokenResponse>> RegisterAsync(RegisterRequest request, string? ipAddress, CancellationToken cancellationToken = default);

    Task<AuthResult<AuthTokenResponse>> LoginAsync(LoginRequest request, string? ipAddress, CancellationToken cancellationToken = default);

    Task<AuthResult<AuthTokenResponse>> RefreshAsync(RefreshRequest request, string? ipAddress, CancellationToken cancellationToken = default);

    Task<AuthResult<bool>> LogoutAsync(LogoutRequest request, CancellationToken cancellationToken = default);

    Task<AuthResult<CurrentUserResponse>> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
