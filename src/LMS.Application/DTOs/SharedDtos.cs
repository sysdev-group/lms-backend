// ─── Submissions ───────────────────────────────────────────────────────────────
namespace LMS.Application.DTOs.Submissions
{
    public class SubmissionDto
    {
        public Guid Id { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
        public bool IsLate { get; set; }
        public string? FileName { get; set; }
        public bool IsGraded { get; set; }
    }

    public class CreateSubmissionRequest
    {
        public Guid AssignmentId { get; set; }
        public Guid? FileId { get; set; }
    }

    public class GradeSubmissionRequest
    {
        public decimal MarksAwarded { get; set; }
        public string? Feedback { get; set; }
    }
}

// ─── Grades ────────────────────────────────────────────────────────────────────
namespace LMS.Application.DTOs.Grades
{
    public class GradeDto
    {
        public Guid Id { get; set; }
        public decimal MarksAwarded { get; set; }
        public string? LetterGrade { get; set; }
        public decimal? GradePoints { get; set; }
        public bool IsPublished { get; set; }
        public string AssignmentTitle { get; set; } = string.Empty;
        public string? Feedback { get; set; }
    }
}

// ─── Notifications ─────────────────────────────────────────────────────────────
namespace LMS.Application.DTOs.Notifications
{
    using LMS.Domain.Enums;

    public class NotificationDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class SendNotificationRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
        public List<Guid> RecipientIds { get; set; } = new();
        public DateTime? ScheduledAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}

// ─── Enrollment ────────────────────────────────────────────────────────────────
namespace LMS.Application.DTOs.Enrollment
{
    public class EnrollmentDto
    {
        public Guid Id { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime EnrolledAt { get; set; }
    }

    public class EnrollStudentRequest
    {
        public Guid StudentId { get; set; }
        public Guid CourseId { get; set; }
        public Guid SemesterId { get; set; }
    }
}

// ─── Timetable ─────────────────────────────────────────────────────────────────
namespace LMS.Application.DTOs.Timetable
{
    public class TimetableSessionDto
    {
        public Guid Id { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public string LecturerName { get; set; } = string.Empty;
        public string Room { get; set; } = string.Empty;
        public string DayOfWeek { get; set; } = string.Empty;
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool IsPublished { get; set; }
    }

    public class CreateSessionRequest
    {
        public Guid CourseId { get; set; }
        public Guid LecturerId { get; set; }
        public Guid SemesterId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string Room { get; set; } = string.Empty;
        public string Type { get; set; } = "Lecture";
    }
}

// ─── Attendance ────────────────────────────────────────────────────────────────
namespace LMS.Application.DTOs.Attendance
{
    using LMS.Domain.Enums;

    public class AttendanceRecordDto
    {
        public Guid StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class MarkAttendanceRequest
    {
        public Guid TimetableSessionId { get; set; }
        public DateOnly Date { get; set; }
        public List<StudentAttendanceEntry> Records { get; set; } = new();
    }

    public class StudentAttendanceEntry
    {
        public Guid StudentId { get; set; }
        public AttendanceStatus Status { get; set; }
        public string? Notes { get; set; }
    }

    public class StudentAttendanceSummaryDto
    {
        public Guid CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public int TotalSessions { get; set; }
        public int AttendedSessions { get; set; }
        public decimal AttendancePercentage { get; set; }
        public bool BelowWarningThreshold { get; set; }
    }
}
