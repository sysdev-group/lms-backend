using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LMS.Application.DTOs.Auth;

/// <summary>Request body for POST /api/v1/auth/login</summary>
public class LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

/// <summary>Response returned on successful login.</summary>
public class LoginResponse
{
    /// <summary>Short-lived JWT access token (15 min). Store in memory only — never localStorage.</summary>
    public string AccessToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public int ExpiresInSeconds { get; set; } = 900; // 15 minutes
    public AuthUserDto User { get; set; } = null!;

    /// <summary>Raw refresh token — written to HttpOnly cookie only, never serialized to JSON.</summary>
    [JsonIgnore]
    public string RefreshToken { get; set; } = string.Empty;
}

/// <summary>Minimal user info returned with the auth token.</summary>
public class AuthUserDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

/// <summary>Request body for POST /api/v1/auth/forgot-password</summary>
public class ForgotPasswordRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}

/// <summary>Request body for POST /api/v1/auth/reset-password</summary>
public class ResetPasswordRequest
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string NewPassword { get; set; } = string.Empty;

    [Required, Compare(nameof(NewPassword))]
    public string ConfirmPassword { get; set; } = string.Empty;
}
