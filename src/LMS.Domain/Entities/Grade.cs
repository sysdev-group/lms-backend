namespace LMS.Domain.Entities;

/// <summary>
/// Represents the grade awarded for a submission.
/// Grade visibility is controlled by IsPublished — hidden from students until lecturer publishes.
/// See Section 7.6 and Section 27.
/// </summary>
public class Grade
{
    public Guid Id { get; set; }
    public decimal MarksAwarded { get; set; }
    public string? LetterGrade { get; set; }   // e.g. "A+", "B", "F"
    public decimal? GradePoints { get; set; }  // e.g. 4.0, 3.3, 0.0
    public bool IsPublished { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PublishedAt { get; set; }

    // Foreign keys
    public Guid SubmissionId { get; set; }

    // Navigation properties
    public Submission Submission { get; set; } = null!;
}
