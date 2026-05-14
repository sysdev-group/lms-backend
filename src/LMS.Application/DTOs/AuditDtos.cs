namespace LMS.Application.DTOs.Audit;

public class AuditLogDto
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public Guid? UserId { get; set; }
    public string? UserRole { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? Before { get; set; }
    public string? After { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
