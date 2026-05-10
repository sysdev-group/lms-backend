using LMS.Application.DTOs.Auth;

namespace LMS.Application.Interfaces;

/// <summary>
/// Contract for all authentication operations.
/// Implementation lives in LMS.Infrastructure/Services/AuthService.cs.
/// This is the WORKED EXAMPLE — study this interface and its implementation
/// before writing your own module's interface.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Validates credentials and issues a JWT access token + refresh token.
    /// On success: access token returned in body, refresh token set as HttpOnly cookie.
    /// </summary>
    Task<LoginResponse> LoginAsync(LoginRequest request, string ipAddress);

    /// <summary>
    /// Revokes the user's current refresh token and invalidates their session.
    /// </summary>
    Task LogoutAsync(Guid userId, string refreshToken);

    /// <summary>
    /// Validates a refresh token cookie and issues a new access token + rotated refresh token.
    /// If the presented token is already revoked, ALL tokens for this user are revoked (reuse attack).
    /// </summary>
    Task<LoginResponse> RefreshTokenAsync(string refreshToken, string ipAddress);

    /// <summary>
    /// Sends a password reset email if the address exists in the system.
    /// Response is always the same regardless of whether the email exists — prevents enumeration.
    /// </summary>
    Task ForgotPasswordAsync(ForgotPasswordRequest request, string ipAddress);

    /// <summary>
    /// Validates the reset token and updates the user's password.
    /// Invalidates all active sessions for the user on success.
    /// </summary>
    Task ResetPasswordAsync(ResetPasswordRequest request, string ipAddress);
}
