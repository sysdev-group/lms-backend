using LMS.Application.DTOs.Submissions;
using LMS.Application.Interfaces;
using LMS.Domain.Entities;
using LMS.Domain.Enums;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Services;

/// <summary>
/// Handles student submissions and lecturer grading. See Section 7.5 and Section 27.
/// </summary>
public class SubmissionService : ISubmissionService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public SubmissionService(AppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Submits an assignment on behalf of a student.
    /// Validates enrollment, deadline policy, and duplicate/resubmission rules before persisting.
    /// Throws <see cref="KeyNotFoundException"/> if the assignment does not exist.
    /// Throws <see cref="UnauthorizedAccessException"/> if the student is not actively enrolled.
    /// Throws <see cref="InvalidOperationException"/> for resubmission or late-submission policy violations.
    /// </summary>
    public async Task<SubmissionDto> SubmitAsync(CreateSubmissionRequest request, Guid studentId)
    {
        var assignment = await _db.Assignments
            .FirstOrDefaultAsync(a => a.Id == request.AssignmentId)
            ?? throw new KeyNotFoundException($"Assignment {request.AssignmentId} not found.");

        await EnsureEnrolledAsync(studentId, assignment.CourseId);

        var existing = await _db.Submissions
            .FirstOrDefaultAsync(s => s.AssignmentId == request.AssignmentId && s.StudentId == studentId);

        if (existing is not null && !assignment.AllowResubmission)
            throw new InvalidOperationException("Resubmission is not allowed for this assignment.");

        var isPastDeadline = DateTime.UtcNow > assignment.Deadline;
        if (isPastDeadline && !assignment.AllowLateSubmission)
            throw new InvalidOperationException("The deadline has passed and late submissions are not allowed.");

        Guid submissionId;

        if (existing is not null)
        {
            existing.SubmittedAt = DateTime.UtcNow;
            existing.IsLate = isPastDeadline;
            existing.Status = SubmissionStatus.Resubmitted;
            existing.FileId = request.FileId;
            existing.GradedAt = null;
            existing.GradedById = null;
            existing.LecturerFeedback = null;
            if (existing.Grade is not null)
                _db.Grades.Remove(existing.Grade);
            submissionId = existing.Id;
        }
        else
        {
            var submission = new Submission
            {
                Id = Guid.NewGuid(),
                AssignmentId = request.AssignmentId,
                StudentId = studentId,
                FileId = request.FileId,
                SubmittedAt = DateTime.UtcNow,
                IsLate = isPastDeadline,
                Status = isPastDeadline ? SubmissionStatus.Late : SubmissionStatus.Submitted
            };
            _db.Submissions.Add(submission);
            submissionId = submission.Id;
        }

        await _db.SaveChangesAsync();
        return await LoadAndMapAsync(submissionId);
    }

    /// <summary>
    /// Returns a single submission by ID.
    /// Students may only view their own submissions. Lecturers may only view submissions
    /// within their own courses. Admins have unrestricted access.
    /// Throws <see cref="KeyNotFoundException"/> if the submission does not exist.
    /// Throws <see cref="UnauthorizedAccessException"/> if the caller lacks access.
    /// </summary>
    public async Task<SubmissionDto> GetByIdAsync(Guid id)
    {
        var submission = await LoadSubmissionOrThrowAsync(id);
        EnsureAccess(submission);
        return MapToDto(submission);
    }

    /// <summary>
    /// Returns all submissions for an assignment. Lecturer and Admin only.
    /// Lecturers may only query assignments in their own courses.
    /// Throws <see cref="UnauthorizedAccessException"/> if the caller is a student or an unrelated lecturer.
    /// Throws <see cref="KeyNotFoundException"/> if the assignment does not exist.
    /// </summary>
    public async Task<List<SubmissionDto>> GetByAssignmentAsync(Guid assignmentId)
    {
        if (_currentUser.Role == UserRole.Student)
            throw new UnauthorizedAccessException("Students cannot view all submissions for an assignment.");

        if (!await _db.Assignments.AnyAsync(a => a.Id == assignmentId))
            throw new KeyNotFoundException($"Assignment {assignmentId} not found.");

        if (_currentUser.Role == UserRole.Lecturer)
            await EnsureLecturerOwnsAssignmentAsync(assignmentId);

        var submissions = await _db.Submissions
            .AsNoTracking()
            .Include(s => s.Student)
            .Include(s => s.File)
            .Include(s => s.Grade)
            .Where(s => s.AssignmentId == assignmentId)
            .ToListAsync();

        return submissions.Select(MapToDto).ToList();
    }

    /// <summary>
    /// Creates or updates the grade for a submission, applying the Section 27.4 grading scale.
    /// Grade starts unpublished; use <c>GradeService.PublishGradeAsync</c> to release it to the student.
    /// Throws <see cref="KeyNotFoundException"/> if the submission does not exist.
    /// Throws <see cref="UnauthorizedAccessException"/> if the caller is a student or an unrelated lecturer.
    /// </summary>
    public async Task<SubmissionDto> GradeSubmissionAsync(Guid submissionId, GradeSubmissionRequest request, Guid gradedById)
    {
        var submission = await LoadSubmissionOrThrowAsync(submissionId);

        if (_currentUser.Role == UserRole.Student)
            throw new UnauthorizedAccessException("Students cannot grade submissions.");

        if (_currentUser.Role == UserRole.Lecturer && submission.Assignment.Course.LecturerId != _currentUser.UserId)
            throw new UnauthorizedAccessException("You do not own this course.");

        if (request.MarksAwarded < 0 || request.MarksAwarded > submission.Assignment.MaxMarks)
            throw new ArgumentException($"MarksAwarded must be between 0 and {submission.Assignment.MaxMarks}.");

        var percentage = request.MarksAwarded / submission.Assignment.MaxMarks * 100;
        var (letter, points) = ComputeGrade(percentage);

        if (submission.Grade is null)
        {
            _db.Grades.Add(new Grade
            {
                Id = Guid.NewGuid(),
                SubmissionId = submissionId,
                MarksAwarded = request.MarksAwarded,
                LetterGrade = letter,
                GradePoints = points,
                IsPublished = false,
                CreatedAt = DateTime.UtcNow
            });
        }
        else
        {
            submission.Grade.MarksAwarded = request.MarksAwarded;
            submission.Grade.LetterGrade = letter;
            submission.Grade.GradePoints = points;
        }

        submission.Status = SubmissionStatus.Graded;
        submission.GradedAt = DateTime.UtcNow;
        submission.GradedById = gradedById;
        submission.LecturerFeedback = request.Feedback;

        await _db.SaveChangesAsync();
        return await LoadAndMapAsync(submissionId);
    }

    // ─── Private helpers ──────────────────────────────────────────────────────

    private async Task EnsureEnrolledAsync(Guid studentId, Guid courseId)
    {
        var enrolled = await _db.Enrollments.AnyAsync(e =>
            e.StudentId == studentId &&
            e.CourseId == courseId &&
            e.Status == EnrollmentStatus.Active);

        if (!enrolled)
            throw new UnauthorizedAccessException("You are not enrolled in this course.");
    }

    private async Task EnsureLecturerOwnsAssignmentAsync(Guid assignmentId)
    {
        var owns = await _db.Assignments.AnyAsync(a =>
            a.Id == assignmentId &&
            a.Course.LecturerId == _currentUser.UserId);

        if (!owns)
        {
            var exists = await _db.Assignments.AnyAsync(a => a.Id == assignmentId);
            if (!exists) throw new KeyNotFoundException($"Assignment {assignmentId} not found.");
            throw new UnauthorizedAccessException("You do not own this course.");
        }
    }

    private void EnsureAccess(Submission submission)
    {
        var role = _currentUser.Role;
        var userId = _currentUser.UserId;

        if (role == UserRole.Admin) return;

        if (role == UserRole.Student && submission.StudentId != userId)
            throw new UnauthorizedAccessException("You can only view your own submissions.");

        if (role == UserRole.Lecturer && submission.Assignment.Course.LecturerId != userId)
            throw new UnauthorizedAccessException("You do not own this course.");
    }

    private async Task<Submission> LoadSubmissionOrThrowAsync(Guid id) =>
        await _db.Submissions
            .Include(s => s.Student)
            .Include(s => s.File)
            .Include(s => s.Grade)
            .Include(s => s.Assignment).ThenInclude(a => a.Course)
            .FirstOrDefaultAsync(s => s.Id == id)
        ?? throw new KeyNotFoundException($"Submission {id} not found.");

    private async Task<SubmissionDto> LoadAndMapAsync(Guid submissionId) =>
        MapToDto(await LoadSubmissionOrThrowAsync(submissionId));

    private static SubmissionDto MapToDto(Submission s) => new()
    {
        Id = s.Id,
        StudentName = $"{s.Student.FirstName} {s.Student.LastName}",
        Status = s.Status.ToString(),
        SubmittedAt = s.SubmittedAt,
        IsLate = s.IsLate,
        FileName = s.File?.OriginalName,
        IsGraded = s.Grade is not null
    };

    private static (string letter, decimal points) ComputeGrade(decimal percentage) => percentage switch
    {
        >= 90 => ("A+", 4.0m),
        >= 85 => ("A",  4.0m),
        >= 80 => ("A-", 3.7m),
        >= 75 => ("B+", 3.3m),
        >= 70 => ("B",  3.0m),
        >= 65 => ("B-", 2.7m),
        >= 60 => ("C+", 2.3m),
        >= 55 => ("C",  2.0m),
        >= 50 => ("D",  1.0m),
        _     => ("F",  0.0m)
    };
}
