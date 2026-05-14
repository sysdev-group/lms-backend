using LMS.Application.Common;
using LMS.Application.DTOs.Audit;
using LMS.Application.Interfaces;
using LMS.Domain.Entities;
using LMS.Domain.Enums;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LMS.Infrastructure.Services;

/// <summary>
/// Writes and queries audit log entries. See Section 32 — Audit Log Module.
/// Audit logs are immutable: they may be read and written, never deleted.
/// </summary>
public class AuditService : IAuditService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<AuditService> _logger;

    public AuditService(AppDbContext db, ICurrentUserService currentUser, ILogger<AuditService> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    /// <summary>
    /// Persists a single audit log entry to the database.
    /// Safe to call from any service — failures are caught and logged internally, never propagated.
    /// </summary>
    public async Task LogAsync(string action, string entityType, string? entityId, Guid? userId,
        string? userRole, string? before, string? after, string? ipAddress, string? userAgent)
    {
        try
        {
            _db.AuditLogs.Add(new AuditLog
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                UserId = userId,
                UserRole = userRole,
                Before = before,
                After = after,
                IpAddress = ipAddress,
                UserAgent = userAgent
            });

            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Audit log write failed: {Action} on {EntityType}/{EntityId}", action, entityType, entityId);
        }
    }

    /// <summary>
    /// Returns a paginated, filtered list of audit log entries.
    /// All filter parameters are optional and combinable.
    /// </summary>
    /// <exception cref="UnauthorizedAccessException">Thrown when the caller is not an Admin.</exception>
    public async Task<PaginatedResult<object>> QueryLogsAsync(string? entityType, string? action,
        Guid? userId, DateTime? from, DateTime? to, int page, int pageSize)
    {
        if (_currentUser.Role != UserRole.Admin)
            throw new UnauthorizedAccessException("Audit log access is restricted to Admins.");

        var q = _db.AuditLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(entityType))
            q = q.Where(l => l.EntityType == entityType);

        if (!string.IsNullOrWhiteSpace(action))
            q = q.Where(l => l.Action == action);

        if (userId.HasValue)
            q = q.Where(l => l.UserId == userId.Value);

        if (from.HasValue)
            q = q.Where(l => l.Timestamp >= from.Value);

        if (to.HasValue)
            q = q.Where(l => l.Timestamp <= to.Value);

        var totalCount = await q.CountAsync();

        var logs = await q
            .OrderByDescending(l => l.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResult<object>
        {
            Items = logs.Select(l => (object)MapToDto(l)).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    private static AuditLogDto MapToDto(AuditLog l) => new()
    {
        Id = l.Id,
        Timestamp = l.Timestamp,
        UserId = l.UserId,
        UserRole = l.UserRole,
        Action = l.Action,
        EntityType = l.EntityType,
        EntityId = l.EntityId,
        Before = l.Before,
        After = l.After,
        IpAddress = l.IpAddress,
        UserAgent = l.UserAgent
    };
}
