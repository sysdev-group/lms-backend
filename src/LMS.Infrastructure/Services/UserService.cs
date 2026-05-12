using LMS.Application.Common;
using LMS.Application.DTOs.Auth;
using LMS.Application.DTOs.Users;
using LMS.Application.Interfaces;
using LMS.Domain.Entities;
using LMS.Domain.Enums;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Services;

/// <summary>
/// Implements user management: account creation, role assignment, deactivation,
/// admin-initiated password reset triggers, and bulk CSV import.
/// </summary>
public class UserService : IUserService
{
    private readonly AppDbContext _db;
    private readonly IAuthService _authService;

    public UserService(AppDbContext db, IAuthService authService)
    {
        _db = db;
        _authService = authService;
    }

    /// <summary>
    /// Returns a paginated, optionally filtered list of all users.
    /// Supports search by name or email, filter by role, and filter by active status.
    /// </summary>
    public async Task<PaginatedResult<UserDto>> GetUsersAsync(UserQueryParams query)
    {
        var q = _db.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.ToLowerInvariant();
            q = q.Where(u =>
                u.Email.ToLower().Contains(term) ||
                u.FirstName.ToLower().Contains(term) ||
                u.LastName.ToLower().Contains(term));
        }

        if (query.Role.HasValue)
            q = q.Where(u => u.Role == query.Role.Value);

        if (query.IsActive.HasValue)
            q = q.Where(u => u.IsActive == query.IsActive.Value);

        var totalCount = await q.CountAsync();

        var users = await q
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return new PaginatedResult<UserDto>
        {
            Items = users.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        };
    }

    /// <summary>
    /// Returns a single user by their unique identifier.
    /// Throws <see cref="KeyNotFoundException"/> if the user does not exist.
    /// </summary>
    public async Task<UserDto> GetByIdAsync(Guid id)
    {
        var user = await _db.Users.FindAsync(id)
            ?? throw new KeyNotFoundException($"User {id} not found.");

        return MapToDto(user);
    }

    /// <summary>
    /// Creates a new user account with a randomly generated temporary password.
    /// The user must complete a password reset before their first login.
    /// Throws <see cref="InvalidOperationException"/> if the email is already registered.
    /// </summary>
    public async Task<UserDto> CreateUserAsync(CreateUserRequest request)
    {
        var email = request.Email.ToLowerInvariant();

        if (await _db.Users.AnyAsync(u => u.Email == email))
            throw new InvalidOperationException($"A user with email '{email}' already exists.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = email,
            Role = request.Role,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(GenerateTemporaryPassword()),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // TODO: Send welcome email with password-reset link via IEmailService. See Section 22.
        return MapToDto(user);
    }

    /// <summary>
    /// Applies a partial update to an existing user.
    /// Only non-null fields in the request are applied.
    /// Throws <see cref="KeyNotFoundException"/> if the user does not exist.
    /// Throws <see cref="InvalidOperationException"/> if the new email is already taken.
    /// </summary>
    public async Task<UserDto> UpdateUserAsync(Guid id, UpdateUserRequest request)
    {
        var user = await _db.Users.FindAsync(id)
            ?? throw new KeyNotFoundException($"User {id} not found.");

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var email = request.Email.ToLowerInvariant();
            if (await _db.Users.AnyAsync(u => u.Email == email && u.Id != id))
                throw new InvalidOperationException($"Email '{email}' is already in use.");
            user.Email = email;
        }

        if (!string.IsNullOrWhiteSpace(request.FirstName))
            user.FirstName = request.FirstName.Trim();

        if (!string.IsNullOrWhiteSpace(request.LastName))
            user.LastName = request.LastName.Trim();

        if (request.Role.HasValue)
            user.Role = request.Role.Value;

        if (request.IsActive.HasValue)
            user.IsActive = request.IsActive.Value;

        await _db.SaveChangesAsync();
        return MapToDto(user);
    }

    /// <summary>
    /// Soft-deactivates a user account by setting <c>IsActive = false</c>.
    /// The user record is never hard-deleted.
    /// Throws <see cref="KeyNotFoundException"/> if the user does not exist.
    /// </summary>
    public async Task DeactivateUserAsync(Guid id)
    {
        var user = await _db.Users.FindAsync(id)
            ?? throw new KeyNotFoundException($"User {id} not found.");

        user.IsActive = false;
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Triggers a password reset email for the given user.
    /// Delegates to <see cref="IAuthService.ForgotPasswordAsync"/> using the user's registered email.
    /// Throws <see cref="KeyNotFoundException"/> if the user does not exist.
    /// </summary>
    public async Task TriggerPasswordResetAsync(Guid userId)
    {
        var user = await _db.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException($"User {userId} not found.");

        await _authService.ForgotPasswordAsync(
            new ForgotPasswordRequest { Email = user.Email },
            ipAddress: "admin-initiated");
    }

    /// <summary>
    /// Bulk-imports users from a UTF-8 CSV stream.
    /// Expected format (with header row): <c>FirstName,LastName,Email,Role</c>.
    /// Rows with emails that already exist in the system are skipped without error.
    /// Validation errors are collected per-row and returned rather than aborting the import.
    /// Note: quoted fields containing commas are not supported.
    /// </summary>
    public async Task<BulkImportResult> BulkImportAsync(Stream csvStream)
    {
        var result = new BulkImportResult();
        var knownEmails = (await _db.Users.Select(u => u.Email).ToListAsync()).ToHashSet();
        var usersToAdd = new List<User>();

        using var reader = new StreamReader(csvStream);
        await reader.ReadLineAsync(); // skip header

        string? line;
        var lineNumber = 1;

        while ((line = await reader.ReadLineAsync()) is not null)
        {
            lineNumber++;
            if (string.IsNullOrWhiteSpace(line)) continue;

            var cols = line.Split(',');
            if (cols.Length < 4)
            {
                result.Errors.Add($"Line {lineNumber}: expected 4 columns (FirstName,LastName,Email,Role).");
                continue;
            }

            var firstName = cols[0].Trim();
            var lastName  = cols[1].Trim();
            var email     = cols[2].Trim().ToLowerInvariant();
            var roleRaw   = cols[3].Trim();

            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(email))
            {
                result.Errors.Add($"Line {lineNumber}: FirstName, LastName, and Email are required.");
                continue;
            }

            if (!Enum.TryParse<UserRole>(roleRaw, ignoreCase: true, out var role))
            {
                result.Errors.Add($"Line {lineNumber}: unknown role '{roleRaw}'. Valid values: Student, Lecturer, Admin.");
                continue;
            }

            // knownEmails.Add returns false if the email was already in the set —
            // this also guards against duplicates within the CSV itself
            if (!knownEmails.Add(email))
            {
                result.Skipped++;
                continue;
            }

            usersToAdd.Add(new User
            {
                Id = Guid.NewGuid(),
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Role = role,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(GenerateTemporaryPassword()),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            result.Created++;
        }

        if (usersToAdd.Count > 0)
        {
            _db.Users.AddRange(usersToAdd);
            await _db.SaveChangesAsync();
        }

        return result;
    }

    // ─── Private helpers ──────────────────────────────────────────────────────

    private static UserDto MapToDto(User u) => new()
    {
        Id = u.Id,
        FirstName = u.FirstName,
        LastName = u.LastName,
        Email = u.Email,
        Role = u.Role.ToString(),
        IsActive = u.IsActive,
        CreatedAt = u.CreatedAt,
        LastLoginAt = u.LastLoginAt
    };

    private static string GenerateTemporaryPassword()
    {
        // 12-char alphanumeric temp password — user resets via the emailed link
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789";
        var bytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(12);
        return new string(bytes.Select(b => chars[b % chars.Length]).ToArray());
    }
}
