namespace LMS.Domain.Entities;

/// <summary>
/// Represents a JWT refresh token issued to a user device/session.
/// Tokens are rotated on each use — the old token is invalidated and a new one issued.
/// Reuse of a revoked token triggers revocation of ALL user tokens. See Section 30.
/// </summary>
public class RefreshToken
{
    public Guid Id { get; set; }

    /// <summary>
    /// SHA-256 hash of the token value. Never store raw tokens.
    /// </summary>
    public string TokenHash { get; set; } = string.Empty;

    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? DeviceInfo { get; set; }
    public Guid? ReplacedByTokenId { get; set; }

    // Foreign keys
    public Guid UserId { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt.HasValue;
    public bool IsActive => !IsExpired && !IsRevoked;
}
