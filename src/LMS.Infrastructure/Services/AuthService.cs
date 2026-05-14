using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using LMS.Application.DTOs.Auth;
using LMS.Application.Interfaces;
using LMS.Domain.Entities;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace LMS.Infrastructure.Services;

/// <summary>
/// Fully implemented Auth service — the worked example for this project.
/// Every other service should follow the same pattern:
///   1. Constructor-inject AppDbContext and any config/services needed
///   2. Implement each interface method
///   3. Use async/await throughout
///   4. Throw meaningful exceptions — the middleware catches them
/// </summary>
public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    /// <inheritdoc />
    public async Task<LoginResponse> LoginAsync(LoginRequest request, string ipAddress)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLowerInvariant() && u.IsActive);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        user.LastLoginAt = DateTime.UtcNow;

        var accessToken = GenerateAccessToken(user);
        var (refreshToken, rawToken) = await CreateRefreshTokenAsync(user.Id, ipAddress);

        await _db.SaveChangesAsync();

        return BuildLoginResponse(accessToken, rawToken, user);
    }

    /// <inheritdoc />
    public async Task LogoutAsync(Guid userId, string refreshToken)
    {
        var tokenHash = HashToken(refreshToken);
        var token = await _db.RefreshTokens
            .FirstOrDefaultAsync(t => t.UserId == userId && t.TokenHash == tokenHash);

        if (token is not null && token.IsActive)
        {
            token.RevokedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }

    /// <inheritdoc />
    public async Task<LoginResponse> RefreshTokenAsync(string refreshToken, string ipAddress)
    {
        var tokenHash = HashToken(refreshToken);
        var token = await _db.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);

        if (token is null)
            throw new UnauthorizedAccessException("Invalid refresh token.");

        // Reuse detection — revoked token presented again → revoke all user tokens
        if (token.IsRevoked)
        {
            await RevokeAllUserTokensAsync(token.UserId);
            throw new UnauthorizedAccessException("Refresh token reuse detected. All sessions invalidated.");
        }

        if (token.IsExpired)
            throw new UnauthorizedAccessException("Refresh token expired. Please log in again.");

        // Rotate — revoke old, issue new
        token.RevokedAt = DateTime.UtcNow;

        var (newRefreshToken, newRawToken) = await CreateRefreshTokenAsync(token.UserId, ipAddress);
        token.ReplacedByTokenId = newRefreshToken.Id;

        var accessToken = GenerateAccessToken(token.User);
        await _db.SaveChangesAsync();

        return BuildLoginResponse(accessToken, newRawToken, token.User);
    }

    /// <inheritdoc />
    public async Task ForgotPasswordAsync(ForgotPasswordRequest request, string ipAddress)
    {
        // Always return success — never reveal whether email exists (prevents enumeration)
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLowerInvariant());

        if (user is null) return;

        // TODO: Generate reset token, store it, send email via IEmailService
        // Pattern: generate GUID token, hash it, store hash with 30-min expiry, email raw token
        // See Section 29 of system documentation for full spec
        await Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task ResetPasswordAsync(ResetPasswordRequest request, string ipAddress)
    {
        // TODO: Validate reset token, update password, revoke all refresh tokens
        // See Section 29 of system documentation for full spec
        await Task.CompletedTask;
        throw new NotImplementedException("Password reset — to be implemented. See Section 29.");
    }

    // ─── Private helpers ──────────────────────────────────────────────────────

    private string GenerateAccessToken(User user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Secret"]
                ?? throw new InvalidOperationException("JWT secret not configured.")));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),  // 15-minute access tokens per Section 30
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<(RefreshToken token, string rawToken)> CreateRefreshTokenAsync(Guid userId, string ipAddress)
    {
        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = HashToken(rawToken),
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            DeviceInfo = ipAddress
        };

        _db.RefreshTokens.Add(refreshToken);

        return await Task.FromResult((refreshToken, rawToken));
    }

    private async Task RevokeAllUserTokensAsync(Guid userId)
    {
        var tokens = await _db.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ToListAsync();

        foreach (var token in tokens)
            token.RevokedAt = DateTime.UtcNow;
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static LoginResponse BuildLoginResponse(string accessToken, string rawRefreshToken, User user) => new()
    {
        AccessToken = accessToken,
        RefreshToken = rawRefreshToken,
        User = new AuthUserDto
        {
            Id = user.Id,
            FullName = $"{user.FirstName} {user.LastName}",
            Email = user.Email,
            Role = user.Role.ToString()
        }
    };
}
