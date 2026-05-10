using LMS.Domain.Enums;

namespace LMS.Domain.Entities;

/// <summary>
/// Represents a student's submission for an assignment.
/// Tracks submission time to detect late submissions automatically. See Section 7.5.
/// </summary>
public class Submission
{
    public Guid Id { get; set; }
    public SubmissionStatus Status { get; set; } = SubmissionStatus.Submitted;
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public bool IsLate { get; set; } = false;
    public string? LecturerFeedback { get; set; }
    public DateTime? GradedAt { get; set; }

    // Foreign keys
    public Guid AssignmentId { get; set; }
    public Guid StudentId { get; set; }
    public Guid? FileId { get; set; }
    public Guid? GradedById { get; set; }

    // Navigation properties
    public Assignment Assignment { get; set; } = null!;
    public User Student { get; set; } = null!;
    public FileRecord? File { get; set; }
    public User? GradedBy { get; set; }
    public Grade? Grade { get; set; }
}
