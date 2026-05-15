// ─────────────────────────────────────────────────────────────────────────────
// STUB CONTROLLERS
//
// Each controller below:
//   - Inherits BaseController (gets CurrentUserId, CurrentUserRole, IpAddress, ApiOk helpers)
//   - Has correct [Route], [Authorize], and [HttpVerb] attributes
//   - Returns 501 Not Implemented until the service method is filled in
//   - Has XML doc comments for Swagger
//
// HOW TO IMPLEMENT YOUR CONTROLLER:
//   1. Inject your service interface in the constructor
//   2. Call the service method and return ApiOk(result)
//   3. Add [Authorize(Roles = "Admin")] or specific role restrictions as needed
//   4. Keep action methods under 10 lines — all logic belongs in the service
// ─────────────────────────────────────────────────────────────────────────────

using LMS.Application.DTOs.Assignments;
using LMS.Application.DTOs.Attendance;
using LMS.Application.DTOs.Submissions;
using LMS.Application.DTOs.Courses;
using LMS.Application.DTOs.Enrollment;
using LMS.Application.DTOs.Notifications;
using LMS.Application.DTOs.Timetable;
using LMS.Application.DTOs.Users;
using LMS.Application.Interfaces;
using LMS.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.API.Controllers;

// ─── USERS ────────────────────────────────────────────────────────────────────
/// <summary>User management — Admin only (except profile endpoints).</summary>
[Route("api/v1/users")]
[Authorize]
public class UsersController : BaseController
{
    private readonly IUserService _userService;
    public UsersController(IUserService userService) { _userService = userService; }

    /// <summary>Get a paginated list of all users. Admin only.</summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll([FromQuery] UserQueryParams query)
    {
        var result = await _userService.GetUsersAsync(query);
        return ApiOk(result);
    }

    /// <summary>Get a single user by ID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _userService.GetByIdAsync(id);
        return ApiOk(result);
    }

    /// <summary>Create a new user. Admin only.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        var result = await _userService.CreateUserAsync(request);
        return ApiCreated(result);
    }

    /// <summary>Update a user's details. Admin only.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest request)
    {
        var result = await _userService.UpdateUserAsync(id, request);
        return ApiOk(result);
    }

    /// <summary>Deactivate a user account. Admin only.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        await _userService.DeactivateUserAsync(id);
        return ApiNoContent();
    }
}

// ─── COURSES ──────────────────────────────────────────────────────────────────
/// <summary>Course management. Admin creates; Lecturers and Students view.</summary>
[Route("api/v1/courses")]
[Authorize]
public class CoursesController : BaseController
{
    private readonly ICourseService _courseService;
    public CoursesController(ICourseService courseService) { _courseService = courseService; }

    /// <summary>Get courses visible to the current user based on their role.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] CourseQueryParams query)
    {
        var result = await _courseService.GetCoursesAsync(query, CurrentUserId, CurrentUserRole);
        return ApiOk(result);
    }

    /// <summary>Get a single course by ID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _courseService.GetByIdAsync(id);
        return ApiOk(result);
    }

    /// <summary>Get all students enrolled in this course. Lecturer or Admin only.</summary>
    [HttpGet("{id:guid}/students")]
    [Authorize(Roles = "Lecturer,Admin")]
    public async Task<IActionResult> GetStudents(Guid id)
    {
        var result = await _courseService.GetEnrolledStudentsAsync(id);
        return ApiOk(result);
    }

    /// <summary>Create a new course. Admin only.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateCourseRequest request)
    {
        var result = await _courseService.CreateCourseAsync(request);
        return ApiCreated(result);
    }

    /// <summary>Update a course. Admin only.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCourseRequest request)
    {
        var result = await _courseService.UpdateCourseAsync(id, request);
        return ApiOk(result);
    }
}

// ─── ASSIGNMENTS ──────────────────────────────────────────────────────────────
/// <summary>Assignment management within courses.</summary>
[Route("api/v1/assignments")]
[Authorize]
public class AssignmentsController : BaseController
{
    private readonly IAssignmentService _assignmentService;
    private readonly ISubmissionService _submissionService;

    public AssignmentsController(IAssignmentService assignmentService, ISubmissionService submissionService)
    {
        _assignmentService = assignmentService;
        _submissionService = submissionService;
    }

    /// <summary>Get all assignments for a course.</summary>
    [HttpGet]
    public async Task<IActionResult> GetByCourse([FromQuery] Guid courseId)
    {
        var result = await _assignmentService.GetByCourseAsync(courseId);
        return ApiOk(result);
    }

    /// <summary>Get a single assignment.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _assignmentService.GetByIdAsync(id);
        return ApiOk(result);
    }

    /// <summary>Create an assignment. Lecturer only.</summary>
    [HttpPost]
    [Authorize(Roles = "Lecturer,Admin")]
    public async Task<IActionResult> Create([FromBody] CreateAssignmentRequest request)
    {
        var result = await _assignmentService.CreateAssignmentAsync(request, CurrentUserId);
        return ApiCreated(result);
    }

    /// <summary>Grade a submission. Lecturer only.</summary>
    [HttpPut("{id:guid}/grade")]
    [Authorize(Roles = "Lecturer,Admin")]
    public async Task<IActionResult> Grade(Guid id, [FromBody] GradeSubmissionRequest request)
    {
        var result = await _submissionService.GradeSubmissionAsync(id, request, CurrentUserId);
        return ApiOk(result);
    }
}

// ─── SUBMISSIONS ──────────────────────────────────────────────────────────────
/// <summary>Student submission management.</summary>
[Route("api/v1/submissions")]
[Authorize]
public class SubmissionsController : BaseController
{
    private readonly ISubmissionService _submissionService;
    private readonly AppDbContext _db;

    public SubmissionsController(ISubmissionService submissionService, AppDbContext db)
    {
        _submissionService = submissionService;
        _db = db;
    }

    /// <summary>Submit an assignment. Student only.</summary>
    [HttpPost]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> Submit([FromBody] CreateSubmissionRequest request)
    {
        var result = await _submissionService.SubmitAsync(request, CurrentUserId);
        return ApiCreated(result);
    }

    /// <summary>Get a single submission.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _submissionService.GetByIdAsync(id);
        return ApiOk(result);
    }

    /// <summary>Get all submissions for an assignment. Lecturer or Admin only.</summary>
    [HttpGet("assignment/{assignmentId:guid}")]
    [Authorize(Roles = "Lecturer,Admin")]
    public async Task<IActionResult> GetByAssignment(Guid assignmentId)
    {
        var result = await _submissionService.GetByAssignmentAsync(assignmentId);
        return ApiOk(result);
    }

    /// <summary>Get all submissions made by the currently authenticated student.</summary>
    [HttpGet("my")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetMine()
    {
        var result = await _db.Submissions
            .AsNoTracking()
            .Where(s => s.StudentId == CurrentUserId)
            .Select(s => new SubmissionDto
            {
                Id = s.Id,
                StudentName = s.Student.FirstName + " " + s.Student.LastName,
                Status = s.Status.ToString(),
                SubmittedAt = s.SubmittedAt,
                IsLate = s.IsLate,
                FileName = s.File != null ? s.File.OriginalName : null,
                IsGraded = s.Grade != null
            })
            .ToListAsync();
        return ApiOk(result);
    }
}

// ─── NOTIFICATIONS ────────────────────────────────────────────────────────────
/// <summary>In-app notification management.</summary>
[Route("api/v1/notifications")]
[Authorize]
public class NotificationsController : BaseController
{
    private readonly INotificationService _notificationService;
    public NotificationsController(INotificationService notificationService) { _notificationService = notificationService; }

    /// <summary>Get all notifications for the current user.</summary>
    [HttpGet]
    public async Task<IActionResult> GetMine([FromQuery] bool unreadOnly = false)
    {
        var result = await _notificationService.GetForUserAsync(CurrentUserId, unreadOnly);
        return ApiOk(result);
    }

    /// <summary>Send a notification to recipients. Lecturer or Admin only.</summary>
    [HttpPost]
    [Authorize(Roles = "Lecturer,Admin")]
    public async Task<IActionResult> Send([FromBody] SendNotificationRequest request)
    {
        await _notificationService.SendAsync(request, CurrentUserId);
        return ApiOk<object?>(null, "Notification sent.");
    }

    /// <summary>Mark a notification as read.</summary>
    [HttpPatch("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id)
    {
        await _notificationService.MarkAsReadAsync(id, CurrentUserId);
        return ApiNoContent();
    }

    /// <summary>Mark all notifications as read for the current user.</summary>
    [HttpPatch("mark-all-read")]
    [Authorize]
    public async Task<IActionResult> MarkAllAsRead()
    {
        await _notificationService.MarkAllAsReadAsync(CurrentUserId);
        return ApiOk<string>("All notifications marked as read.");
    }
}

// ─── TIMETABLE ────────────────────────────────────────────────────────────────
/// <summary>Timetable session management and publishing.</summary>
[Route("api/v1/timetable")]
[Authorize]
public class TimetableController : BaseController
{
    private readonly ITimetableService _timetableService;
    public TimetableController(ITimetableService timetableService) { _timetableService = timetableService; }

    /// <summary>Get all timetable sessions visible to the current user (role-filtered).</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _timetableService.GetByBatchAsync(Guid.Empty);
        return ApiOk(result);
    }

    /// <summary>Get timetable sessions for a semester/batch.</summary>
    [HttpGet("batch/{semesterId:guid}")]
    public async Task<IActionResult> GetByBatch(Guid semesterId)
    {
        var result = await _timetableService.GetByBatchAsync(semesterId);
        return ApiOk(result);
    }

    /// <summary>Create a timetable session. Admin only. Conflict detection runs automatically.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateSessionRequest request)
    {
        var result = await _timetableService.CreateSessionAsync(request);
        return ApiCreated(result);
    }

    /// <summary>Publish a session — makes it visible to students and lecturers.</summary>
    [HttpPut("{id:guid}/publish")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Publish(Guid id)
    {
        await _timetableService.PublishSessionAsync(id);
        return ApiOk<object?>(null, "Session published.");
    }

    /// <summary>Delete an unpublished timetable session. Lecturer (own sessions) or Admin.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Lecturer,Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _timetableService.DeleteSessionAsync(id);
        return ApiNoContent();
    }
}

// ─── ATTENDANCE ───────────────────────────────────────────────────────────────
/// <summary>Attendance recording and reporting.</summary>
[Route("api/v1/attendance")]
[Authorize]
public class AttendanceController : BaseController
{
    private readonly IAttendanceService _attendanceService;
    public AttendanceController(IAttendanceService attendanceService) { _attendanceService = attendanceService; }

    /// <summary>Mark attendance for a session. Lecturer only.</summary>
    [HttpPost("session/{sessionId:guid}")]
    [Authorize(Roles = "Lecturer")]
    public async Task<IActionResult> Mark(Guid sessionId, [FromBody] MarkAttendanceRequest request)
    {
        await _attendanceService.MarkAttendanceAsync(request, CurrentUserId);
        return ApiOk<object?>(null, "Attendance recorded.");
    }

    /// <summary>Get all attendance records for a specific attendance session. Lecturer or Admin only.</summary>
    [HttpGet("session/{sessionId:guid}")]
    [Authorize(Roles = "Lecturer,Admin")]
    public async Task<IActionResult> GetSessionRecords(Guid sessionId)
    {
        var result = await _attendanceService.GetSessionRecordsAsync(sessionId);
        return ApiOk(result);
    }

    /// <summary>Get attendance summary for a student.</summary>
    [HttpGet("student/{studentId:guid}")]
    public async Task<IActionResult> GetStudentSummary(Guid studentId)
    {
        var result = await _attendanceService.GetStudentSummaryAsync(studentId);
        return ApiOk(result);
    }
}

// ─── ENROLLMENT ───────────────────────────────────────────────────────────────
/// <summary>Student enrollment management.</summary>
[Route("api/v1/enrollment")]
[Authorize]
public class EnrollmentController : BaseController
{
    private readonly IEnrollmentService _enrollmentService;
    public EnrollmentController(IEnrollmentService enrollmentService) { _enrollmentService = enrollmentService; }

    /// <summary>Enroll a student in a course. Admin only.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Enroll([FromBody] EnrollStudentRequest request)
    {
        var result = await _enrollmentService.EnrollStudentAsync(request, CurrentUserId);
        return ApiCreated(result);
    }

    /// <summary>Drop an enrollment. Admin only.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Drop(Guid id)
    {
        await _enrollmentService.DropEnrollmentAsync(id, CurrentUserId);
        return ApiNoContent();
    }

    /// <summary>Get all enrollments for a student.</summary>
    [HttpGet("student/{studentId:guid}")]
    public async Task<IActionResult> GetByStudent(Guid studentId)
    {
        var result = await _enrollmentService.GetByStudentAsync(studentId);
        return ApiOk(result);
    }
}

// ─── SEARCH ───────────────────────────────────────────────────────────────────
/// <summary>System-wide search — results are automatically scoped to the caller's role.</summary>
[Route("api/v1/search")]
[Authorize]
public class SearchController : BaseController
{
    /// <summary>Search across entities. Results filtered by caller's role.</summary>
    [HttpGet]
    public IActionResult Search([FromQuery] string q, [FromQuery] string? type)
    {
        // TODO: Implement full-text search using PostgreSQL tsvector/tsquery. See Section 31.
        throw new NotImplementedException("Search — to be implemented. See Section 31.");
    }
}

// ─── AUDIT ────────────────────────────────────────────────────────────────────
/// <summary>Audit log viewer. Admin only. Read-only.</summary>
[Route("api/v1/audit")]
[Authorize(Roles = "Admin")]
public class AuditController : BaseController
{
    private readonly IAuditService _auditService;
    public AuditController(IAuditService auditService) { _auditService = auditService; }

    /// <summary>Query the audit log with filters. Admin only.</summary>
    [HttpGet]
    public async Task<IActionResult> Query(
        [FromQuery] string? entity,
        [FromQuery] string? action,
        [FromQuery] Guid? userId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _auditService.QueryLogsAsync(entity, action, userId, from, to, page, pageSize);
        return ApiOk(result);
    }
}
