using LMS.Domain.Enums;

namespace LMS.Domain.Entities;

/// <summary>
/// Represents a student's enrollment in a course for a given semester.
/// Soft-deleted on drop — history is retained for academic records. See Section 28.
/// </summary>
public class Enrollment
{
    public Guid Id { get; set; }
    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    public DateTime? DroppedAt { get; set; }

    // Foreign keys
    public Guid StudentId { get; set; }
    public Guid CourseId { get; set; }
    public Guid SemesterId { get; set; }
    public Guid EnrolledById { get; set; }   // Admin or system that created the enrollment
    public Guid? DroppedById { get; set; }

    // Navigation properties
    public User Student { get; set; } = null!;
    public Course Course { get; set; } = null!;
    public Semester Semester { get; set; } = null!;
}
