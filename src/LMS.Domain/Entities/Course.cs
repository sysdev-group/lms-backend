namespace LMS.Domain.Entities;

/// <summary>
/// Represents an academic course/module offered in a semester.
/// Courses are scoped to a Semester and linked to a Lecturer. See Section 7.4.
/// </summary>
public class Course
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CreditHours { get; set; }
    public bool IsArchived { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign keys
    public Guid LecturerId { get; set; }
    public Guid SemesterId { get; set; }

    // Navigation properties
    public User Lecturer { get; set; } = null!;
    public Semester Semester { get; set; } = null!;
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    public ICollection<FileRecord> Materials { get; set; } = new List<FileRecord>();
}
