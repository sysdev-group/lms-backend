namespace LMS.Domain.Entities;

/// <summary>
/// Represents an academic semester. All courses, enrollments, and timetables are scoped to a semester.
/// See Section 34.
/// </summary>
public class Semester
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;         // e.g. "Semester 1 2024/2025"
    public string AcademicYear { get; set; } = string.Empty; // e.g. "2024/2025"
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime EnrollmentOpenDate { get; set; }
    public DateTime EnrollmentCloseDate { get; set; }
    public DateTime GradeSubmissionDeadline { get; set; }
    public string Status { get; set; } = "Upcoming"; // "Upcoming" | "Active" | "Completed"
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<Course> Courses { get; set; } = new List<Course>();
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
