using LMS.Application.DTOs.Enrollment;
using LMS.Application.Interfaces;
using LMS.Domain.Entities;
using LMS.Domain.Enums;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Services;

/// <summary>
/// Handles student enrollment and drop workflows.
/// </summary>
public class EnrollmentService : IEnrollmentService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    /// <summary>
    /// Creates an enrollment service with database and current-user dependencies.
    /// </summary>
    public EnrollmentService(AppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Enrolls a student in a course for a semester.
    /// </summary>
    public async Task<EnrollmentDto> EnrollStudentAsync(EnrollStudentRequest request, Guid enrolledById)
    {
        var student = await _db.Users
            .FirstOrDefaultAsync(u => u.Id == request.StudentId && u.Role == UserRole.Student && u.IsActive)
            ?? throw new KeyNotFoundException($"Student {request.StudentId} not found.");

        var course = await _db.Courses
            .FirstOrDefaultAsync(c => c.Id == request.CourseId)
            ?? throw new KeyNotFoundException($"Course {request.CourseId} not found.");

        if (course.SemesterId != request.SemesterId)
            throw new ArgumentException("Course does not belong to the specified semester.", nameof(request));

        var alreadyEnrolled = await _db.Enrollments.AnyAsync(e =>
            e.StudentId == request.StudentId &&
            e.CourseId == request.CourseId &&
            e.SemesterId == request.SemesterId &&
            e.Status == EnrollmentStatus.Active);

        if (alreadyEnrolled)
            throw new InvalidOperationException("Student is already actively enrolled in this course.");

        var enrollment = new Enrollment
        {
            Id = Guid.NewGuid(),
            StudentId = request.StudentId,
            CourseId = request.CourseId,
            SemesterId = request.SemesterId,
            EnrolledById = enrolledById,
            Status = EnrollmentStatus.Active,
            EnrolledAt = DateTime.UtcNow
        };

        _db.Enrollments.Add(enrollment);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw new InvalidOperationException("Student is already enrolled in this course for the semester.");
        }

        enrollment.Student = student;
        enrollment.Course = course;
        return MapToDto(enrollment);
    }

    /// <summary>
    /// Drops an existing enrollment.
    /// </summary>
    public async Task DropEnrollmentAsync(Guid enrollmentId, Guid droppedById)
    {
        await Task.CompletedTask;
        throw new NotImplementedException("TODO: Soft delete - set Status = Dropped, DroppedAt, DroppedById.");
    }

    /// <summary>
    /// Retrieves enrollments for the specified student.
    /// </summary>
    public async Task<List<EnrollmentDto>> GetByStudentAsync(Guid studentId)
    {
        if (_currentUser.Role == UserRole.Student && _currentUser.UserId != studentId)
            throw new UnauthorizedAccessException("You may only view your own data.");

        var enrollments = await _db.Enrollments
            .AsNoTracking()
            .Include(e => e.Student)
            .Include(e => e.Course)
            .Where(e => e.StudentId == studentId)
            .OrderByDescending(e => e.EnrolledAt)
            .ToListAsync();

        return enrollments.Select(MapToDto).ToList();
    }

    /// <summary>
    /// Retrieves enrollments for the specified course.
    /// </summary>
    public async Task<List<EnrollmentDto>> GetByCourseAsync(Guid courseId)
    {
        var enrollments = await _db.Enrollments
            .AsNoTracking()
            .Include(e => e.Student)
            .Include(e => e.Course)
            .Where(e => e.CourseId == courseId && e.Status == EnrollmentStatus.Active)
            .OrderBy(e => e.Student.LastName)
            .ThenBy(e => e.Student.FirstName)
            .ToListAsync();

        return enrollments.Select(MapToDto).ToList();
    }

    private static EnrollmentDto MapToDto(Enrollment enrollment) => new()
    {
        Id = enrollment.Id,
        StudentName = $"{enrollment.Student.FirstName} {enrollment.Student.LastName}",
        CourseName = enrollment.Course.Title,
        Status = enrollment.Status.ToString(),
        EnrolledAt = enrollment.EnrolledAt
    };
}
