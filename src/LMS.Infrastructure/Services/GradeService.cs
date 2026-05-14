using LMS.Application.DTOs.Grades;
using LMS.Application.Interfaces;
using LMS.Domain.Entities;
using LMS.Domain.Enums;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Services;

/// <summary>
/// Manages grade visibility and publishing. See Section 7.6 and Section 27.
/// </summary>
public class GradeService : IGradeService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GradeService(AppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Returns grades for a student, optionally filtered by course.
    /// Students may only retrieve their own grades and only when <see cref="Grade.IsPublished"/> is true.
    /// Admins and lecturers may retrieve grades for any student.
    /// Throws <see cref="UnauthorizedAccessException"/> if a student requests another student's grades.
    /// </summary>
    public async Task<List<GradeDto>> GetByStudentAsync(Guid studentId, Guid? courseId = null)
    {
        if (_currentUser.Role == UserRole.Student && _currentUser.UserId != studentId)
            throw new UnauthorizedAccessException("Students may only view their own grades.");

        var query = _db.Grades
            .Where(g => g.Submission.StudentId == studentId);

        if (_currentUser.Role == UserRole.Student)
            query = query.Where(g => g.IsPublished);

        if (courseId.HasValue)
            query = query.Where(g => g.Submission.Assignment.CourseId == courseId.Value);

        return await query
            .Select(g => new GradeDto
            {
                Id = g.Id,
                MarksAwarded = g.MarksAwarded,
                LetterGrade = g.LetterGrade,
                GradePoints = g.GradePoints,
                IsPublished = g.IsPublished,
                AssignmentTitle = g.Submission.Assignment.Title,
                Feedback = g.Submission.LecturerFeedback
            })
            .ToListAsync();
    }

    /// <summary>
    /// Returns all grades for a course, published or not.
    /// Restricted to lecturers who own the course and admins.
    /// Throws <see cref="UnauthorizedAccessException"/> if a student or non-owning lecturer calls this.
    /// Throws <see cref="KeyNotFoundException"/> if the course does not exist.
    /// </summary>
    public async Task<List<GradeDto>> GetByCourseAsync(Guid courseId)
    {
        await EnsureLecturerOrAdminCourseAccessAsync(courseId);

        return await _db.Grades
            .Where(g => g.Submission.Assignment.CourseId == courseId)
            .Select(g => new GradeDto
            {
                Id = g.Id,
                MarksAwarded = g.MarksAwarded,
                LetterGrade = g.LetterGrade,
                GradePoints = g.GradePoints,
                IsPublished = g.IsPublished,
                AssignmentTitle = g.Submission.Assignment.Title,
                Feedback = g.Submission.LecturerFeedback
            })
            .ToListAsync();
    }

    /// <summary>
    /// Publishes a single grade and sends an in-app notification to the student.
    /// Throws <see cref="KeyNotFoundException"/> if the grade does not exist.
    /// Throws <see cref="UnauthorizedAccessException"/> if the caller does not own the course.
    /// </summary>
    public async Task PublishGradeAsync(Guid gradeId)
    {
        var grade = await _db.Grades
            .Include(g => g.Submission)
                .ThenInclude(s => s.Assignment)
            .FirstOrDefaultAsync(g => g.Id == gradeId)
            ?? throw new KeyNotFoundException($"Grade {gradeId} not found.");

        await EnsureLecturerOrAdminCourseAccessAsync(grade.Submission.Assignment.CourseId);

        if (!grade.IsPublished)
        {
            grade.IsPublished = true;
            grade.PublishedAt = DateTime.UtcNow;

            _db.Notifications.Add(new Notification
            {
                Id = Guid.NewGuid(),
                Title = "Grade Published",
                Body = $"Your grade for \"{grade.Submission.Assignment.Title}\" has been published.",
                Priority = NotificationPriority.Normal,
                RecipientId = grade.Submission.StudentId,
                SenderId = _currentUser.UserId,
                CreatedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Bulk-publishes all unpublished grades for a course and notifies each affected student.
    /// Throws <see cref="KeyNotFoundException"/> if the course does not exist.
    /// Throws <see cref="UnauthorizedAccessException"/> if the caller does not own the course.
    /// </summary>
    public async Task PublishAllGradesForCourseAsync(Guid courseId)
    {
        await EnsureLecturerOrAdminCourseAccessAsync(courseId);

        var unpublished = await _db.Grades
            .Include(g => g.Submission)
                .ThenInclude(s => s.Assignment)
            .Where(g => g.Submission.Assignment.CourseId == courseId && !g.IsPublished)
            .ToListAsync();

        if (unpublished.Count == 0)
            return;

        var now = DateTime.UtcNow;

        foreach (var grade in unpublished)
        {
            grade.IsPublished = true;
            grade.PublishedAt = now;

            _db.Notifications.Add(new Notification
            {
                Id = Guid.NewGuid(),
                Title = "Grade Published",
                Body = $"Your grade for \"{grade.Submission.Assignment.Title}\" has been published.",
                Priority = NotificationPriority.Normal,
                RecipientId = grade.Submission.StudentId,
                SenderId = _currentUser.UserId,
                CreatedAt = now
            });
        }

        await _db.SaveChangesAsync();
    }

    // ─── Private helpers ──────────────────────────────────────────────────────

    private async Task EnsureLecturerOrAdminCourseAccessAsync(Guid courseId)
    {
        if (_currentUser.Role == UserRole.Student)
            throw new UnauthorizedAccessException("Students are not permitted to access this resource.");

        if (_currentUser.Role == UserRole.Admin)
            return;

        var owns = await _db.Courses.AnyAsync(c => c.Id == courseId && c.LecturerId == _currentUser.UserId);
        if (!owns)
            throw new UnauthorizedAccessException("You do not own this course.");
    }
}
