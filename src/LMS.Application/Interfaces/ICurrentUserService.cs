using LMS.Domain.Enums;

namespace LMS.Application.Interfaces;

/// <summary>
/// Provides identity information for the currently authenticated HTTP request.
/// Implemented by CurrentUserService via IHttpContextAccessor.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>The authenticated user's ID.</summary>
    Guid UserId { get; }

    /// <summary>The authenticated user's role.</summary>
    UserRole Role { get; }
}
