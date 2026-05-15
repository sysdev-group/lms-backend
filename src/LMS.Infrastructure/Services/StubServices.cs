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
using LMS.Application.DTOs.Enrollment;
using LMS.Application.DTOs.Attendance;
using LMS.Application.Interfaces;
using LMS.Domain.Enums;

namespace LMS.Infrastructure.Services;

// ─── ENROLLMENT SERVICE ───────────────────────────────────────────────────────
/// <summary>Docs: Section 28 — Enrollment Workflow</summary>
public class EnrollmentService : IEnrollmentService
{
    private readonly ICurrentUserService _currentUser;
    public EnrollmentService(ICurrentUserService currentUser) { _currentUser = currentUser; }

    public Task<EnrollmentDto> EnrollStudentAsync(EnrollStudentRequest request, Guid enrolledById)
        => throw new NotImplementedException("TODO: Check duplicate, check enrollment deadline, check capacity.");

    public Task DropEnrollmentAsync(Guid enrollmentId, Guid droppedById)
        => throw new NotImplementedException("TODO: Soft delete — set Status = Dropped, DroppedAt, DroppedById.");

    public Task<List<EnrollmentDto>> GetByStudentAsync(Guid studentId)
    {
        if (_currentUser.Role == UserRole.Student && _currentUser.UserId != studentId)
            throw new UnauthorizedAccessException("You may only view your own data.");
        throw new NotImplementedException("TODO: Include course and semester details.");
    }

    public Task<List<EnrollmentDto>> GetByCourseAsync(Guid courseId)
        => throw new NotImplementedException("TODO: Include student details, filter active enrollments.");
}


