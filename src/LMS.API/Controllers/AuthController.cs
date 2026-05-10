using LMS.Application.DTOs.Auth;
using LMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers;

/// <summary>
/// Authentication endpoints — the WORKED EXAMPLE controller.
/// Study this before writing your own controller:
///   - Inject your service via constructor
///   - Use [Authorize] and [AllowAnonymous] appropriately
///   - Always return ApiResponse via base class helpers (ApiOk, ApiCreated)
///   - Keep action methods short — delegate all logic to the service
///   - XML doc comments on every action → they appear in Swagger
/// </summary>
[Route("api/v1/auth")]
public class AuthController : BaseController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Authenticate a user with email and password.
    /// Returns a JWT access token in the response body and sets a refresh token HttpOnly cookie.
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>Access token and basic user info</returns>
    /// <response code="200">Login successful</response>
    /// <response code="401">Invalid credentials</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request, IpAddress);

        // Set refresh token as HttpOnly cookie — never accessible from JavaScript
        SetRefreshTokenCookie(result.AccessToken);

        return ApiOk(result);
    }

    /// <summary>
    /// Log out the current user and revoke their refresh token.
    /// </summary>
    /// <response code="204">Logged out successfully</response>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies["refreshToken"] ?? string.Empty;
        await _authService.LogoutAsync(CurrentUserId, refreshToken);
        Response.Cookies.Delete("refreshToken");

        return ApiNoContent();
    }

    /// <summary>
    /// Obtain a new access token using the refresh token cookie.
    /// The refresh token is automatically rotated on each call.
    /// </summary>
    /// <response code="200">New access token issued</response>
    /// <response code="401">Refresh token invalid or expired</response>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Refresh()
    {
        var refreshToken = Request.Cookies["refreshToken"]
            ?? throw new UnauthorizedAccessException("Refresh token not found.");

        var result = await _authService.RefreshTokenAsync(refreshToken, IpAddress);
        SetRefreshTokenCookie(result.AccessToken);

        return ApiOk(result);
    }

    /// <summary>
    /// Request a password reset email.
    /// Always returns 200 regardless of whether the email exists — prevents user enumeration.
    /// </summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(200)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        await _authService.ForgotPasswordAsync(request, IpAddress);
        return ApiOk<object?>(null, "If this email exists, a reset link has been sent.");
    }

    /// <summary>
    /// Complete a password reset using the token from the reset email.
    /// </summary>
    /// <response code="200">Password reset successfully</response>
    /// <response code="400">Token invalid or expired</response>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        await _authService.ResetPasswordAsync(request, IpAddress);
        return ApiOk<object?>(null, "Password has been reset. Please log in.");
    }

    // ─── Private helpers ──────────────────────────────────────────────────────

    private void SetRefreshTokenCookie(string token)
    {
        Response.Cookies.Append("refreshToken", token, new CookieOptions
        {
            HttpOnly = true,       // Not accessible from JavaScript
            Secure = true,         // HTTPS only
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });
    }
}
