// ─────────────────────────────────────────────────────────────────────────────
// STUB SERVICE IMPLEMENTATIONS
//
// Each service below is a placeholder that:
//   - Correctly implements its interface (compiles and runs)
//   - Throws NotImplementedException on every method
//   - Has a comment pointing to the relevant docs section
//
// HOW TO IMPLEMENT YOUR SERVICE:
//   1. Inject AppDbContext in the constructor (see AuthService.cs for the pattern)
//   2. Replace each throw with real EF Core queries
//   3. Map domain entities to DTOs before returning
//   4. Keep methods under ~30 lines — extract helpers if needed
//   5. Always use async/await — never .Result or .Wait()
// ─────────────────────────────────────────────────────────────────────────────

using LMS.Application.Common;
using LMS.Application.DTOs.Users;
using LMS.Application.DTOs.Courses;
using LMS.Application.DTOs.Assignments;
using LMS.Application.DTOs.Submissions;
using LMS.Application.DTOs.Grades;
using LMS.Application.DTOs.Notifications;
using LMS.Application.DTOs.Enrollment;
using LMS.Application.DTOs.Timetable;
using LMS.Application.DTOs.Attendance;
using LMS.Application.Interfaces;

namespace LMS.Infrastructure.Services;


// ─── SUBMISSION SERVICE ───────────────────────────────────────────────────────
/// <summary>Docs: Section 7.5 — Assignment Module (Submissions)</summary>
public class SubmissionService : ISubmissionService
{
    public Task<SubmissionDto> SubmitAsync(CreateSubmissionRequest request, Guid studentId)
        => throw new NotImplementedException("TODO: Check deadline, set IsLate flag, check resubmission rules.");

    public Task<SubmissionDto> GetByIdAsync(Guid id)
        => throw new NotImplementedException("TODO: Verify caller has access — student sees own, lecturer sees course subs.");

    public Task<List<SubmissionDto>> GetByAssignmentAsync(Guid assignmentId)
        => throw new NotImplementedException("TODO: Verify caller is the course lecturer or admin.");

    public Task<SubmissionDto> GradeSubmissionAsync(Guid submissionId, GradeSubmissionRequest request, Guid gradedById)
        => throw new NotImplementedException("TODO: Create Grade record, apply grading scale. See Section 27.");
}

// ─── GRADE SERVICE ────────────────────────────────────────────────────────────
/// <summary>Docs: Section 7.6 + Section 27 — Grading Module</summary>
public class GradeService : IGradeService
{
    public Task<List<GradeDto>> GetByStudentAsync(Guid studentId, Guid? courseId = null)
        => throw new NotImplementedException("TODO: Only return published grades to students. See Section 27.6.");

    public Task<List<GradeDto>> GetByCourseAsync(Guid courseId)
        => throw new NotImplementedException("TODO: Lecturer can see all grades, published or not.");

    public Task PublishGradeAsync(Guid gradeId)
        => throw new NotImplementedException("TODO: Set IsPublished = true, set PublishedAt, trigger notification.");

    public Task PublishAllGradesForCourseAsync(Guid courseId)
        => throw new NotImplementedException("TODO: Bulk publish — publish all unpublished grades for the course.");
}

// ─── NOTIFICATION SERVICE ─────────────────────────────────────────────────────
/// <summary>Docs: Section 7.7 + Section 23 — Notification System</summary>
public class NotificationService : INotificationService
{
    public Task<List<NotificationDto>> GetForUserAsync(Guid userId, bool unreadOnly = false)
        => throw new NotImplementedException("TODO: Filter by RecipientId, optionally filter unread only.");

    public Task SendAsync(SendNotificationRequest request, Guid senderId)
        => throw new NotImplementedException("TODO: Create Notification records for each recipient. See Section 23.");

    public Task MarkAsReadAsync(Guid notificationId, Guid userId)
        => throw new NotImplementedException("TODO: Verify RecipientId matches userId before marking read.");

    public Task MarkAllAsReadAsync(Guid userId)
        => throw new NotImplementedException("TODO: Bulk update all unread notifications for user.");
}

// ─── ENROLLMENT SERVICE ───────────────────────────────────────────────────────
/// <summary>Docs: Section 28 — Enrollment Workflow</summary>
public class EnrollmentService : IEnrollmentService
{
    public Task<EnrollmentDto> EnrollStudentAsync(EnrollStudentRequest request, Guid enrolledById)
        => throw new NotImplementedException("TODO: Check duplicate, check enrollment deadline, check capacity.");

    public Task DropEnrollmentAsync(Guid enrollmentId, Guid droppedById)
        => throw new NotImplementedException("TODO: Soft delete — set Status = Dropped, DroppedAt, DroppedById.");

    public Task<List<EnrollmentDto>> GetByStudentAsync(Guid studentId)
        => throw new NotImplementedException("TODO: Include course and semester details.");

    public Task<List<EnrollmentDto>> GetByCourseAsync(Guid courseId)
        => throw new NotImplementedException("TODO: Include student details, filter active enrollments.");
}

// ─── TIMETABLE SERVICE ────────────────────────────────────────────────────────
/// <summary>Docs: Section 24 — Timetable Management Module</summary>
public class TimetableService : ITimetableService
{
    public Task<List<TimetableSessionDto>> GetByBatchAsync(Guid semesterId, Guid? lecturerId = null)
        => throw new NotImplementedException("TODO: Query sessions by semester, optionally filter by lecturer.");

    public Task<TimetableSessionDto> CreateSessionAsync(CreateSessionRequest request)
        => throw new NotImplementedException("TODO: Run conflict detection before saving. See Section 24.5.");

    public Task PublishSessionAsync(Guid sessionId)
        => throw new NotImplementedException("TODO: Set IsPublished = true, send notifications to enrolled students.");

    public Task DeleteSessionAsync(Guid sessionId)
        => throw new NotImplementedException("TODO: Only allow delete of unpublished sessions.");
}

// ─── ATTENDANCE SERVICE ───────────────────────────────────────────────────────
/// <summary>Docs: Section 26 — Attendance Management Module</summary>
public class AttendanceService : IAttendanceService
{
    public Task MarkAttendanceAsync(MarkAttendanceRequest request, Guid lecturerId)
        => throw new NotImplementedException("TODO: Create AttendanceSession, then AttendanceRecords per student.");

    public Task<List<AttendanceRecordDto>> GetSessionRecordsAsync(Guid attendanceSessionId)
        => throw new NotImplementedException("TODO: Return all student records for this session.");

    public Task<List<StudentAttendanceSummaryDto>> GetStudentSummaryAsync(Guid studentId)
        => throw new NotImplementedException("TODO: Calculate attendance % per course. See Section 26.6 for formula.");
}

// ─── AUDIT SERVICE ────────────────────────────────────────────────────────────
/// <summary>Docs: Section 32 — Audit Log Module</summary>
public class AuditService : IAuditService
{
    public Task LogAsync(string action, string entityType, string? entityId, Guid? userId,
        string? userRole, string? before, string? after, string? ipAddress, string? userAgent)
        => throw new NotImplementedException("TODO: Create AuditLog record. Never throw on audit failure — catch internally.");

    public Task<PaginatedResult<object>> QueryLogsAsync(string? entityType, string? action,
        Guid? userId, DateTime? from, DateTime? to, int page, int pageSize)
        => throw new NotImplementedException("TODO: Query AuditLogs with filters, paginate results.");
}
