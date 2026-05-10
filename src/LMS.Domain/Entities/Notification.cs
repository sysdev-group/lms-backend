using LMS.Domain.Enums;

namespace LMS.Domain.Entities;

/// <summary>
/// Represents an in-app notification sent to a user.
/// Supports priority levels and read/unread tracking. See Section 7.7 and Section 23.
/// </summary>
public class Notification
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
    public DateTime? ReadAt { get; set; }

    // Foreign keys
    public Guid RecipientId { get; set; }
    public Guid? SenderId { get; set; }   // Null for system-generated notifications

    // Navigation properties
    public User Recipient { get; set; } = null!;
    public User? Sender { get; set; }
}
