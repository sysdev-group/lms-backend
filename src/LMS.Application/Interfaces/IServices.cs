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
using LMS.Application.DTOs.Audit;

namespace LMS.Application.Interfaces;

// ─── USER SERVICE ─────────────────────────────────────────────────────────────
/// <summary>
/// Manages user accounts. Admin only for most operations.
/// TO IMPLEMENT: LMS.Infrastructure/Services/UserService.cs
/// </summary>
public interface IUserService
{
    Task<PaginatedResult<UserDto>> GetUsersAsync(UserQueryParams query);
    Task<UserDto> GetByIdAsync(Guid id);
    Task<UserDto> CreateUserAsync(CreateUserRequest request);
    Task<UserDto> UpdateUserAsync(Guid id, UpdateUserRequest request);
    Task DeactivateUserAsync(Guid id);
    Task TriggerPasswordResetAsync(Guid userId); // Admin-initiated reset

    /// <summary>
    /// Imports users in bulk from a CSV stream.
    /// Expected columns (with header): FirstName,LastName,Email,Role.
    /// Rows with duplicate emails are skipped rather than rejected.
    /// </summary>
    Task<BulkImportResult> BulkImportAsync(Stream csvStream);
}

// ─── COURSE SERVICE ───────────────────────────────────────────────────────────
/// <summary>
/// Manages course creation, configuration, and archival.
/// TO IMPLEMENT: LMS.Infrastructure/Services/CourseService.cs
/// </summary>
public interface ICourseService
{
    Task<PaginatedResult<CourseDto>> GetCoursesAsync(CourseQueryParams query, Guid requestingUserId, string role);
    Task<CourseDto> GetByIdAsync(Guid id);
    Task<CourseDto> CreateCourseAsync(CreateCourseRequest request);
    Task<CourseDto> UpdateCourseAsync(Guid id, UpdateCourseRequest request);
    Task ArchiveCourseAsync(Guid id);
    Task<List<UserDto>> GetEnrolledStudentsAsync(Guid courseId);
}

// ─── ASSIGNMENT SERVICE ───────────────────────────────────────────────────────
/// <summary>
/// Manages assignment creation and configuration within courses.
/// TO IMPLEMENT: LMS.Infrastructure/Services/AssignmentService.cs
/// </summary>
public interface IAssignmentService
{
    Task<List<AssignmentDto>> GetByCourseAsync(Guid courseId);
    Task<AssignmentDto> GetByIdAsync(Guid id);
    Task<AssignmentDto> CreateAssignmentAsync(CreateAssignmentRequest request, Guid lecturerId);
    Task<AssignmentDto> UpdateAssignmentAsync(Guid id, UpdateAssignmentRequest request);
    Task DeleteAssignmentAsync(Guid id);
}

// ─── SUBMISSION SERVICE ───────────────────────────────────────────────────────
/// <summary>
/// Handles student submissions and lecturer grading.
/// TO IMPLEMENT: LMS.Infrastructure/Services/SubmissionService.cs
/// </summary>
public interface ISubmissionService
{
    Task<SubmissionDto> SubmitAsync(CreateSubmissionRequest request, Guid studentId);
    Task<SubmissionDto> GetByIdAsync(Guid id);
    Task<List<SubmissionDto>> GetByAssignmentAsync(Guid assignmentId);
    Task<SubmissionDto> GradeSubmissionAsync(Guid submissionId, GradeSubmissionRequest request, Guid gradedById);
}

// ─── GRADE SERVICE ────────────────────────────────────────────────────────────
/// <summary>
/// Handles grade visibility, publishing, and GPA calculation.
/// TO IMPLEMENT: LMS.Infrastructure/Services/GradeService.cs
/// </summary>
public interface IGradeService
{
    Task<List<GradeDto>> GetByStudentAsync(Guid studentId, Guid? courseId = null);
    Task<List<GradeDto>> GetByCourseAsync(Guid courseId);
    Task PublishGradeAsync(Guid gradeId);
    Task PublishAllGradesForCourseAsync(Guid courseId);
}

// ─── NOTIFICATION SERVICE ─────────────────────────────────────────────────────
/// <summary>
/// Sends and manages in-app notifications.
/// TO IMPLEMENT: LMS.Infrastructure/Services/NotificationService.cs
/// </summary>
public interface INotificationService
{
    Task<List<NotificationDto>> GetForUserAsync(Guid userId, bool unreadOnly = false);
    Task SendAsync(SendNotificationRequest request, Guid senderId);
    Task MarkAsReadAsync(Guid notificationId, Guid userId);
    Task MarkAllAsReadAsync(Guid userId);
}

// ─── ENROLLMENT SERVICE ───────────────────────────────────────────────────────
/// <summary>
/// Handles student enrollment and drop workflows.
/// TO IMPLEMENT: LMS.Infrastructure/Services/EnrollmentService.cs
/// </summary>
public interface IEnrollmentService
{
    Task<EnrollmentDto> EnrollStudentAsync(EnrollStudentRequest request, Guid enrolledById);
    Task DropEnrollmentAsync(Guid enrollmentId, Guid droppedById);
    Task<List<EnrollmentDto>> GetByStudentAsync(Guid studentId);
    Task<List<EnrollmentDto>> GetByCourseAsync(Guid courseId);
}

// ─── TIMETABLE SERVICE ────────────────────────────────────────────────────────
/// <summary>
/// Manages timetable sessions and conflict detection.
/// TO IMPLEMENT: LMS.Infrastructure/Services/TimetableService.cs
/// </summary>
public interface ITimetableService
{
    Task<List<TimetableSessionDto>> GetByBatchAsync(Guid semesterId, Guid? lecturerId = null);
    Task<TimetableSessionDto> CreateSessionAsync(CreateSessionRequest request);
    Task PublishSessionAsync(Guid sessionId);
    Task DeleteSessionAsync(Guid sessionId);
}

// ─── ATTENDANCE SERVICE ───────────────────────────────────────────────────────
/// <summary>
/// Records and retrieves attendance data.
/// TO IMPLEMENT: LMS.Infrastructure/Services/AttendanceService.cs
/// </summary>
public interface IAttendanceService
{
    Task MarkAttendanceAsync(MarkAttendanceRequest request, Guid lecturerId);
    Task<List<AttendanceRecordDto>> GetSessionRecordsAsync(Guid attendanceSessionId);
    Task<List<StudentAttendanceSummaryDto>> GetStudentSummaryAsync(Guid studentId);
}

// ─── AUDIT SERVICE ────────────────────────────────────────────────────────────
/// <summary>
/// Writes and queries audit log entries.
/// TO IMPLEMENT: LMS.Infrastructure/Services/AuditService.cs
/// </summary>
public interface IAuditService
{
    Task LogAsync(string action, string entityType, string? entityId, Guid? userId,
        string? userRole, string? before, string? after, string? ipAddress, string? userAgent);
    Task<PaginatedResult<AuditLogDto>> QueryLogsAsync(string? entityType, string? action,
        Guid? userId, DateTime? from, DateTime? to, int page, int pageSize);
}

// ─── FILE SERVICE ─────────────────────────────────────────────────────────────
/// <summary>
/// Handles file storage, retrieval, and access control.
/// TO IMPLEMENT: LMS.Infrastructure/Services/FileService.cs
/// </summary>
public interface IFileService
{
    Task<Guid> UploadAsync(Stream fileStream, string originalName, string mimeType,
        string relatedEntity, Guid entityId, Guid uploadedById);
    Task<(Stream stream, string fileName, string mimeType)> DownloadAsync(Guid fileId, Guid requestingUserId);
    Task DeleteAsync(Guid fileId, Guid requestingUserId);
}
