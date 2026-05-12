using System.Security.Claims;
using LMS.Application.Interfaces;
using LMS.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace LMS.Infrastructure.Services;

/// <summary>
/// Resolves the current user's identity from the active HTTP request context.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public Guid UserId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User
                .FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("User ID claim missing.");

            return Guid.TryParse(claim, out var id)
                ? id
                : throw new UnauthorizedAccessException("User ID claim is not a valid GUID.");
        }
    }

    /// <inheritdoc />
    public UserRole Role
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User
                .FindFirstValue(ClaimTypes.Role)
                ?? throw new UnauthorizedAccessException("Role claim missing.");

            return Enum.TryParse<UserRole>(claim, out var role)
                ? role
                : throw new UnauthorizedAccessException($"Unknown role claim: {claim}");
        }
    }
}
