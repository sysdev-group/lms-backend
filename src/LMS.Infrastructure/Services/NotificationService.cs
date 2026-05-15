using LMS.Application.DTOs.Notifications;
using LMS.Application.Interfaces;
using LMS.Infrastructure.Data;

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
    {
        await Task.CompletedTask;
        throw new NotImplementedException("TODO: Implement notification retrieval.");
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
        await Task.CompletedTask;
        throw new NotImplementedException("TODO: Implement notification read-state update.");
    }

    /// <summary>
    /// Marks all notifications as read for the specified user.
    /// </summary>
    public async Task MarkAllAsReadAsync(Guid userId)
    {
        await Task.CompletedTask;
        throw new NotImplementedException("TODO: Implement bulk notification read-state update.");
    }
}
