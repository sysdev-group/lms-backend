namespace LMS.Infrastructure.Entities;

/// <summary>
/// Stores a hashed, single-use password reset token.
/// Lives in Infrastructure because Domain/Application layers are read-only.
/// </summary>
public class PasswordResetToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    /// <summary>SHA-256 hex of the raw 64-char token sent in the email. Never store raw.</summary>
    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? IpAddress { get; set; }
}
