namespace LMS.Domain.Entities;

/// <summary>
/// Represents an assignment within a course.
/// Supports configurable resubmission and late submission detection. See Section 7.5.
/// </summary>
public class Assignment
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime Deadline { get; set; }
    public int MaxMarks { get; set; }
    public bool AllowResubmission { get; set; } = false;
    public bool AllowLateSubmission { get; set; } = false;
    public bool TurnitinEnabled { get; set; } = false;
    public bool AllowStudentsViewReport { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign keys
    public Guid CourseId { get; set; }
    public Guid CreatedById { get; set; }

    // Navigation properties
    public Course Course { get; set; } = null!;
    public User CreatedBy { get; set; } = null!;
    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}
