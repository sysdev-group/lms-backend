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
using LMS.Application.DTOs.Notifications;
using LMS.Application.DTOs.Enrollment;
using LMS.Application.DTOs.Attendance;
using LMS.Application.Interfaces;

namespace LMS.Infrastructure.Services;

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

