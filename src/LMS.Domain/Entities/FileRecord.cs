namespace LMS.Domain.Entities;

/// <summary>
/// Metadata record for every file stored in the system.
/// Files are never served directly — always through authenticated download endpoints. See Section 21.
/// </summary>
public class FileRecord
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;       // Sanitised stored name
    public string OriginalName { get; set; } = string.Empty;   // User's original filename
    public string Path { get; set; } = string.Empty;           // Storage path or cloud URL
    public long SizeBytes { get; set; }
    public string MimeType { get; set; } = string.Empty;
    public string RelatedEntity { get; set; } = string.Empty;  // "course" | "assignment" | "submission" | "profile"
    public Guid EntityId { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // Foreign keys
    public Guid UploadedById { get; set; }

    // Navigation properties
    public User UploadedBy { get; set; } = null!;
}
