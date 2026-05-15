using LMS.Application.DTOs.Notifications;
using LMS.Application.Interfaces;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Services;

/// <summary>
/// Handles in-app notification retrieval, creation, and read-state updates.
/// Business logic is intentionally pending; this class establishes the concrete service structure.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly AppDbContext _db;

    /// <summary>
    /// Creates a notification service with access to the LMS database context.
    /// </summary>
    public NotificationService(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Retrieves notifications for a user, optionally limited to unread notifications.
    /// </summary>
    public async Task<List<NotificationDto>> GetForUserAsync(Guid userId, bool unreadOnly = false)
        => await GetNotificationsAsync(userId, unreadOnly);

    /// <summary>
    /// Retrieves notifications for the specified user ordered from newest to oldest.
    /// </summary>
    private async Task<List<NotificationDto>> GetNotificationsAsync(Guid userId, bool unreadOnly)
    {
        var query = _db.Notifications
            .AsNoTracking()
            .Where(n => n.RecipientId == userId);

        if (unreadOnly)
            query = query.Where(n => !n.IsRead);

        var notifications = await query
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        return notifications.Select(MapToDto).ToList();
    }

    /// <summary>
    /// Creates notifications for one or more recipients.
    /// </summary>
    public async Task SendAsync(SendNotificationRequest request, Guid senderId)
    {
        await Task.CompletedTask;
        throw new NotImplementedException("TODO: Implement notification creation.");
    }

    /// <summary>
    /// Marks a single notification as read for the specified user.
    /// </summary>
    public async Task MarkAsReadAsync(Guid notificationId, Guid userId)
    {
        var notification = await _db.Notifications.FindAsync(notificationId)
            ?? throw new KeyNotFoundException($"Notification {notificationId} not found.");

        if (notification.RecipientId != userId)
            throw new UnauthorizedAccessException("You may only mark your own notifications as read.");

        if (notification.IsRead)
            return;

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Marks all notifications as read for the specified user.
    /// </summary>
    public async Task MarkAllAsReadAsync(Guid userId)
    {
        await Task.CompletedTask;
        throw new NotImplementedException("TODO: Implement bulk notification read-state update.");
    }

    private static NotificationDto MapToDto(Domain.Entities.Notification notification) => new()
    {
        Id = notification.Id,
        Title = notification.Title,
        Body = notification.Body,
        Priority = notification.Priority.ToString(),
        IsRead = notification.IsRead,
        CreatedAt = notification.CreatedAt
    };
}
