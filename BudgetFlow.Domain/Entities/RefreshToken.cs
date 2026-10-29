using BudgetFlow.Domain.Common;

namespace BudgetFlow.Domain.Entities;

/// <summary>
/// Represents a hashed refresh token issued to an application user, supporting
/// rotation and revocation for JWT bearer authentication.
/// </summary>
public class RefreshToken : Entity
{
    public Guid ApplicationUserId { get; private set; }

    /// <summary>
    /// SHA-256 hash of the raw refresh token value. The raw value is never persisted.
    /// </summary>
    public string TokenHash { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset ExpiresAtUtc { get; private set; }

    public DateTimeOffset? RevokedAtUtc { get; private set; }

    /// <summary>
    /// The identifier of the refresh token that replaced this one, when rotated.
    /// </summary>
    public Guid? ReplacedByTokenId { get; private set; }

    public string? CreatedByIp { get; private set; }

    public string? RevokedByIp { get; private set; }

    public bool IsRevoked => RevokedAtUtc.HasValue;

    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAtUtc;

    public bool IsActive => !IsRevoked && !IsExpired;

    private RefreshToken()
    {
    }

    public RefreshToken(Guid applicationUserId, string tokenHash, DateTimeOffset expiresAtUtc, string? createdByIp = null)
    {
        ApplicationUserId = applicationUserId;
        TokenHash = tokenHash;
        ExpiresAtUtc = expiresAtUtc;
        CreatedByIp = createdByIp;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Revoke(Guid? replacedByTokenId = null, string? revokedByIp = null)
    {
        RevokedAtUtc = DateTimeOffset.UtcNow;
        ReplacedByTokenId = replacedByTokenId;
        RevokedByIp = revokedByIp;
    }
}
