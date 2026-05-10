namespace LMS.Domain.Entities;

/// <summary>
/// Represents a single scheduled session (lecture, lab, tutorial) in a timetable.
/// Conflict detection runs before any session is saved. See Section 24.
/// </summary>
public class TimetableSession
{
    public Guid Id { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string Room { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;  // "Lecture" | "Lab" | "Tutorial"
    public bool IsPublished { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign keys
    public Guid CourseId { get; set; }
    public Guid LecturerId { get; set; }
    public Guid SemesterId { get; set; }

    // Navigation properties
    public Course Course { get; set; } = null!;
    public User Lecturer { get; set; } = null!;
    public Semester Semester { get; set; } = null!;
    public ICollection<AttendanceSession> AttendanceSessions { get; set; } = new List<AttendanceSession>();
}
