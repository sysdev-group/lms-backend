namespace LMS.Domain.Entities;

/// <summary>
/// Immutable audit log entry. Records every significant action in the system.
/// Audit logs must never be deleted — only archived after 12 months. See Section 32.
/// </summary>
public class AuditLog
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Guid? UserId { get; set; }       // Null for system-generated events
    public string? UserRole { get; set; }
    public string Action { get; set; } = string.Empty;        // e.g. "GRADE_UPDATED"
    public string EntityType { get; set; } = string.Empty;    // e.g. "Submission"
    public string? EntityId { get; set; }
    public string? Before { get; set; }     // JSON snapshot of state before change
    public string? After { get; set; }      // JSON snapshot of state after change
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
