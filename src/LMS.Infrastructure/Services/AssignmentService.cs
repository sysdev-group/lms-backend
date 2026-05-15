using LMS.Application.DTOs.Assignments;
using LMS.Application.Interfaces;
using LMS.Domain.Entities;
using LMS.Domain.Enums;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Services;

/// <summary>
/// Manages assignment creation and configuration within courses. See Section 7.5.
/// </summary>
public class AssignmentService : IAssignmentService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _audit;

    public AssignmentService(AppDbContext db, ICurrentUserService currentUser, IAuditService audit)
    {
        _db = db;
        _currentUser = currentUser;
        _audit = audit;
    }

    /// <summary>
    /// Returns all assignments for a course, scoped by the caller's role.
    /// Students must be actively enrolled; lecturers must own the course; admins see all.
    /// Uses a SQL projection so submission counts never load full rows.
    /// </summary>
    public async Task<List<AssignmentDto>> GetByCourseAsync(Guid courseId)
    {
        await EnsureCourseAccessAsync(courseId);

        return await _db.Assignments
            .Where(a => a.CourseId == courseId)
            .Select(a => new AssignmentDto
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                Deadline = a.Deadline,
                IsDeadlinePassed = DateTime.UtcNow > a.Deadline,
                MaxMarks = a.MaxMarks,
                AllowResubmission = a.AllowResubmission,
                AllowLateSubmission = a.AllowLateSubmission,
                TurnitinEnabled = a.TurnitinEnabled,
                CourseName = a.Course.Title,
                SubmissionCount = a.Submissions.Count()
            })
            .ToListAsync();
    }

    /// <summary>
    /// Returns a single assignment by ID. Includes whether the deadline has already passed.
    /// Throws <see cref="KeyNotFoundException"/> if the assignment does not exist.
    /// </summary>
    public async Task<AssignmentDto> GetByIdAsync(Guid id)
    {
        var assignment = await _db.Assignments
            .Include(a => a.Course)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new KeyNotFoundException($"Assignment {id} not found.");

        await EnsureCourseAccessAsync(assignment.CourseId);

        var submissionCount = await _db.Submissions.CountAsync(s => s.AssignmentId == id);
        return MapToDto(assignment, submissionCount);
    }

    /// <summary>
    /// Creates an assignment in the specified course.
    /// Throws <see cref="ArgumentException"/> if the deadline is not in the future.
    /// Throws <see cref="UnauthorizedAccessException"/> if the lecturer does not own the course.
    /// </summary>
    public async Task<AssignmentDto> CreateAssignmentAsync(CreateAssignmentRequest request, Guid lecturerId)
    {
        if (request.Deadline <= DateTime.UtcNow)
            throw new ArgumentException("Deadline must be in the future.");

        var course = await _db.Courses.FindAsync(request.CourseId)
            ?? throw new KeyNotFoundException($"Course {request.CourseId} not found.");

        if (_currentUser.Role == UserRole.Lecturer && course.LecturerId != lecturerId)
            throw new UnauthorizedAccessException("You do not own this course.");

        var assignment = new Assignment
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            Deadline = request.Deadline,
            MaxMarks = request.MaxMarks,
            AllowResubmission = request.AllowResubmission,
            AllowLateSubmission = request.AllowLateSubmission,
            TurnitinEnabled = request.TurnitinEnabled,
            CourseId = request.CourseId,
            CreatedById = lecturerId,
            CreatedAt = DateTime.UtcNow
        };

        _db.Assignments.Add(assignment);
        await _db.SaveChangesAsync();

        await _audit.LogAsync("Create", "Assignment", assignment.Id.ToString(), _currentUser.UserId,
            _currentUser.Role.ToString(), null, $"Assignment '{assignment.Title}' created", null, null);

        assignment.Course = course;
        return MapToDto(assignment, submissionCount: 0);
    }

    /// <summary>
    /// Partially updates an assignment. Deadline changes are blocked once any submission exists.
    /// Lecturers may only update assignments in their own courses.
    /// Throws <see cref="ArgumentException"/> if a deadline change is attempted after submissions.
    /// Throws <see cref="UnauthorizedAccessException"/> if the caller does not own the assignment.
    /// </summary>
    public async Task<AssignmentDto> UpdateAssignmentAsync(Guid id, UpdateAssignmentRequest request)
    {
        var assignment = await _db.Assignments
            .Include(a => a.Course)
            .FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new KeyNotFoundException($"Assignment {id} not found.");

        if (_currentUser.Role == UserRole.Lecturer && assignment.Course.LecturerId != _currentUser.UserId)
            throw new UnauthorizedAccessException("You do not own this assignment.");

        if (request.Deadline.HasValue && await _db.Submissions.AnyAsync(s => s.AssignmentId == id))
            throw new ArgumentException("Cannot change deadline after submissions exist.");

        if (request.Title is not null) assignment.Title = request.Title;
        if (request.Description is not null) assignment.Description = request.Description;
        if (request.Deadline.HasValue) assignment.Deadline = request.Deadline.Value;
        if (request.AllowResubmission.HasValue) assignment.AllowResubmission = request.AllowResubmission.Value;
        if (request.AllowLateSubmission.HasValue) assignment.AllowLateSubmission = request.AllowLateSubmission.Value;

        await _db.SaveChangesAsync();

        await _audit.LogAsync("Update", "Assignment", assignment.Id.ToString(), _currentUser.UserId,
            _currentUser.Role.ToString(), null, $"Assignment '{assignment.Title}' updated", null, null);

        var submissionCount = await _db.Submissions.CountAsync(s => s.AssignmentId == id);
        return MapToDto(assignment, submissionCount);
    }

    /// <summary>
    /// Deletes an assignment. Blocked if any student submissions exist.
    /// Lecturers may only delete assignments in their own courses.
    /// Throws <see cref="ArgumentException"/> if submissions are present.
    /// Throws <see cref="UnauthorizedAccessException"/> if the caller does not own the assignment.
    /// </summary>
    public async Task DeleteAssignmentAsync(Guid id)
    {
        var assignment = await _db.Assignments
            .Include(a => a.Course)
            .FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new KeyNotFoundException($"Assignment {id} not found.");

        if (_currentUser.Role == UserRole.Lecturer && assignment.Course.LecturerId != _currentUser.UserId)
            throw new UnauthorizedAccessException("You do not own this assignment.");

        if (await _db.Submissions.AnyAsync(s => s.AssignmentId == id))
            throw new ArgumentException("Cannot delete an assignment that has submissions.");

        _db.Assignments.Remove(assignment);
        await _db.SaveChangesAsync();
    }

    // ─── Private helpers ──────────────────────────────────────────────────────

    private async Task EnsureCourseAccessAsync(Guid courseId)
    {
        var userId = _currentUser.UserId;
        var role = _currentUser.Role;

        if (role == UserRole.Admin) return;

        if (role == UserRole.Lecturer)
        {
            var owns = await _db.Courses.AnyAsync(c => c.Id == courseId && c.LecturerId == userId);
            if (!owns)
                throw new UnauthorizedAccessException("You do not have access to this course.");
            return;
        }

        var enrolled = await _db.Enrollments.AnyAsync(e =>
            e.StudentId == userId &&
            e.CourseId == courseId &&
            e.Status == EnrollmentStatus.Active);

        if (!enrolled)
            throw new UnauthorizedAccessException("You are not enrolled in this course.");
    }

    private static AssignmentDto MapToDto(Assignment a, int submissionCount) => new()
    {
        Id = a.Id,
        Title = a.Title,
        Description = a.Description,
        Deadline = a.Deadline,
        IsDeadlinePassed = DateTime.UtcNow > a.Deadline,
        MaxMarks = a.MaxMarks,
        AllowResubmission = a.AllowResubmission,
        AllowLateSubmission = a.AllowLateSubmission,
        TurnitinEnabled = a.TurnitinEnabled,
        CourseName = a.Course?.Title ?? string.Empty,
        SubmissionCount = submissionCount
    };
}
