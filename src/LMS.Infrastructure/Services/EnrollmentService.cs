using LMS.Application.DTOs.Enrollment;
using LMS.Application.Interfaces;
using LMS.Domain.Enums;
using LMS.Infrastructure.Data;

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
        await Task.CompletedTask;
        throw new NotImplementedException("TODO: Check duplicate, check enrollment deadline, check capacity.");
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

        await Task.CompletedTask;
        throw new NotImplementedException("TODO: Include course and semester details.");
    }

    /// <summary>
    /// Retrieves enrollments for the specified course.
    /// </summary>
    public async Task<List<EnrollmentDto>> GetByCourseAsync(Guid courseId)
    {
        await Task.CompletedTask;
        throw new NotImplementedException("TODO: Include student details, filter active enrollments.");
    }
}
