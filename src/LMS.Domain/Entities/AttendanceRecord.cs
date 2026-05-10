using LMS.Domain.Enums;

namespace LMS.Domain.Entities;

/// <summary>
/// Represents one attendance-taking event for a specific timetable session on a given date.
/// A TimetableSession recurs weekly — each occurrence gets its own AttendanceSession. See Section 26.
/// </summary>
public class AttendanceSession
{
    public Guid Id { get; set; }
    public DateOnly Date { get; set; }
    public bool IsClosed { get; set; } = false;
    public DateTime TakenAt { get; set; } = DateTime.UtcNow;

    // Foreign keys
    public Guid TimetableSessionId { get; set; }
    public Guid LecturerId { get; set; }

    // Navigation properties
    public TimetableSession TimetableSession { get; set; } = null!;
    public User Lecturer { get; set; } = null!;
    public ICollection<AttendanceRecord> Records { get; set; } = new List<AttendanceRecord>();
}

/// <summary>
/// Represents a single student's attendance status for one attendance session.
/// Admin overrides are tracked via OverriddenById for audit purposes. See Section 26.4.
/// </summary>
public class AttendanceRecord
{
    public Guid Id { get; set; }
    public AttendanceStatus Status { get; set; }
    public string? Notes { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

    // Foreign keys
    public Guid AttendanceSessionId { get; set; }
    public Guid StudentId { get; set; }
    public Guid? OverriddenById { get; set; }

    // Navigation properties
    public AttendanceSession AttendanceSession { get; set; } = null!;
    public User Student { get; set; } = null!;
    public User? OverriddenBy { get; set; }
}
