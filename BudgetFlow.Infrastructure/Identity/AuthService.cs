using BudgetFlow.Application.Features.Authentication;
using BudgetFlow.Domain.Entities;
using BudgetFlow.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BudgetFlow.Infrastructure.Identity;

/// <summary>
/// Orchestrates user registration, login, refresh-token rotation, logout, and
/// current-user lookup on top of ASP.NET Core Identity and <see cref="ITokenService"/>.
/// </summary>
public sealed class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly BudgetFlowDbContext _dbContext;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        BudgetFlowDbContext dbContext,
        ITokenService tokenService,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<AuthResult<AuthTokenResponse>> RegisterAsync(RegisterRequest request, string? ipAddress, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
        {
            return AuthResult<AuthTokenResponse>.Failure("An account with this email address already exists.");
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            DisplayName = request.DisplayName,
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            return AuthResult<AuthTokenResponse>.Failure(createResult.Errors.Select(e => e.Description));
        }

        _logger.LogInformation("Registered new user {UserId}.", user.Id);

        return await IssueTokensAsync(user, ipAddress, cancellationToken);
    }

    public async Task<AuthResult<AuthTokenResponse>> LoginAsync(LoginRequest request, string? ipAddress, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return AuthResult<AuthTokenResponse>.Failure("Invalid email or password.");
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
        {
            return AuthResult<AuthTokenResponse>.Failure("Invalid email or password.");
        }

        return await IssueTokensAsync(user, ipAddress, cancellationToken);
    }

    public async Task<AuthResult<AuthTokenResponse>> RefreshAsync(RefreshRequest request, string? ipAddress, CancellationToken cancellationToken = default)
    {
        var tokenHash = _tokenService.HashRefreshToken(request.RefreshToken);

        var existingToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

        if (existingToken is null || !existingToken.IsActive)
        {
            return AuthResult<AuthTokenResponse>.Failure("The refresh token is invalid, expired, or has already been used.");
        }

        var user = await _userManager.FindByIdAsync(existingToken.ApplicationUserId.ToString());
        if (user is null)
        {
            return AuthResult<AuthTokenResponse>.Failure("The refresh token is invalid, expired, or has already been used.");
        }

        // Rotate: revoke the presented token and issue a brand-new pair.
        var newTokens = await IssueTokensAsync(user, ipAddress, cancellationToken, revokeExisting: existingToken);

        return newTokens;
    }

    public async Task<AuthResult<bool>> LogoutAsync(LogoutRequest request, CancellationToken cancellationToken = default)
    {
        var tokenHash = _tokenService.HashRefreshToken(request.RefreshToken);

        var existingToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

        if (existingToken is null)
        {
            // Logout is idempotent: an already-invalid token is not an error.
            return AuthResult<bool>.Success(true);
        }

        if (existingToken.IsActive)
        {
            existingToken.Revoke();
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return AuthResult<bool>.Success(true);
    }

    public async Task<AuthResult<CurrentUserResponse>> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return AuthResult<CurrentUserResponse>.Failure("User not found.");
        }

        return AuthResult<CurrentUserResponse>.Success(
            new CurrentUserResponse(user.Id, user.Email ?? string.Empty, user.DisplayName ?? string.Empty));
    }

    private async Task<AuthResult<AuthTokenResponse>> IssueTokensAsync(
        ApplicationUser user,
        string? ipAddress,
        CancellationToken cancellationToken,
        RefreshToken? revokeExisting = null)
    {
        var roles = await _userManager.GetRolesAsync(user);

        var subject = new TokenSubject(user.Id, user.Email ?? string.Empty, roles.ToArray());
        var (accessToken, accessTokenExpiresAtUtc) = _tokenService.CreateAccessToken(subject);

        var rawRefreshToken = _tokenService.CreateRefreshTokenValue();
        var refreshTokenHash = _tokenService.HashRefreshToken(rawRefreshToken);
        var refreshTokenExpiresAtUtc = DateTimeOffset.UtcNow.Add(_tokenService.RefreshTokenLifetime);

        var newRefreshToken = new RefreshToken(user.Id, refreshTokenHash, refreshTokenExpiresAtUtc, ipAddress);

        if (revokeExisting is not null)
        {
            revokeExisting.Revoke(newRefreshToken.Id, ipAddress);
        }

        _dbContext.RefreshTokens.Add(newRefreshToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var response = new AuthTokenResponse(accessToken, accessTokenExpiresAtUtc, rawRefreshToken, refreshTokenExpiresAtUtc);
        return AuthResult<AuthTokenResponse>.Success(response);
    }
}
