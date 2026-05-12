using System.ComponentModel.DataAnnotations;
using LMS.Domain.Enums;

namespace LMS.Application.DTOs.Users;

/// <summary>Full user record returned to Admin.</summary>
public class UserDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

/// <summary>Request body for POST /api/v1/users (Admin creates a new user)</summary>
public class CreateUserRequest
{
    [Required] public string FirstName { get; set; } = string.Empty;
    [Required] public string LastName { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] public UserRole Role { get; set; }
}

/// <summary>Request body for PUT /api/v1/users/{id}</summary>
public class UpdateUserRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public UserRole? Role { get; set; }
    public bool? IsActive { get; set; }
}

/// <summary>Query parameters for GET /api/v1/users</summary>
public class UserQueryParams
{
    public string? Search { get; set; }
    public UserRole? Role { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>Result of a bulk CSV import operation.</summary>
public class BulkImportResult
{
    public int Created { get; set; }
    public int Skipped { get; set; }
    public List<string> Errors { get; set; } = new();
}
