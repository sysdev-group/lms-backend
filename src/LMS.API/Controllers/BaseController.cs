using System.Security.Claims;
using LMS.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers;

/// <summary>
/// Base controller that all API controllers inherit from.
/// Provides shared helpers for response building and extracting the calling user's identity.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public abstract class BaseController : ControllerBase
{
    /// <summary>Returns the authenticated user's ID from their JWT claims.</summary>
    protected Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID claim missing."));

    /// <summary>Returns the authenticated user's role from their JWT claims.</summary>
    protected string CurrentUserRole =>
        User.FindFirstValue(ClaimTypes.Role)
            ?? throw new UnauthorizedAccessException("Role claim missing.");

    /// <summary>Returns the client's IP address for audit and rate limiting purposes.</summary>
    protected string IpAddress =>
        HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    protected OkObjectResult ApiOk<T>(T data, string message = "Success") =>
        Ok(ApiResponse<T>.Ok(data, message));

    protected ObjectResult ApiCreated<T>(T data, string message = "Created") =>
        StatusCode(201, ApiResponse<T>.Ok(data, message));

    protected NoContentResult ApiNoContent() => NoContent();
}
