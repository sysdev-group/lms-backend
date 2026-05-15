using LMS.Application.Common;
using LMS.Application.DTOs.Courses;
using LMS.Application.DTOs.Users;
using LMS.Application.Interfaces;
using LMS.Domain.Entities;
using LMS.Domain.Enums;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Services;

/// <summary>
/// Handles course querying and creation.
/// </summary>
public class CourseService : ICourseService
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;
    private readonly ICurrentUserService _currentUser;

    public CourseService(AppDbContext db, IAuditService audit, ICurrentUserService currentUser)
    {
        _db = db;
        _audit = audit;
        _currentUser = currentUser;
    }

    /// <inheritdoc />
    public async Task<PaginatedResult<CourseDto>> GetCoursesAsync(
        CourseQueryParams query,
        Guid requestingUserId,
        string role)
    {
        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, PaginationConstants.MaxPageSize);
        var courses = ApplyFilters(BaseCourseQuery(), query, requestingUserId, role);

        var totalCount = await courses.CountAsync();
        var items = await courses
            .OrderBy(c => c.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResult<CourseDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    /// <inheritdoc />
    public async Task<CourseDto> GetByIdAsync(Guid id)
    {
        var course = await BaseCourseQuery()
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new KeyNotFoundException($"Course {id} not found.");

        return MapToDto(course);
    }

    /// <inheritdoc />
    public async Task<CourseDto> CreateCourseAsync(CreateCourseRequest request)
    {
        ValidateCreateRequest(request);
        await ValidateCourseReferencesAsync(request);

        var course = new Course
        {
            Id = Guid.NewGuid(),
            Code = request.Code.Trim(),
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            CreditHours = request.CreditHours,
            LecturerId = request.LecturerId,
            SemesterId = request.SemesterId
        };

        _db.Courses.Add(course);
        await _db.SaveChangesAsync();

        await _audit.LogAsync("Create", "Course", course.Id.ToString(), _currentUser.UserId,
            _currentUser.Role.ToString(), null, $"Course '{course.Title}' created", null, null);

        return await GetByIdAsync(course.Id);
    }

    /// <inheritdoc />
    public async Task<CourseDto> UpdateCourseAsync(Guid id, UpdateCourseRequest request)
    {
        var course = await _db.Courses.FindAsync(id)
            ?? throw new KeyNotFoundException($"Course {id} not found.");

        if (request.LecturerId.HasValue)
        {
            var lecturerExists = await _db.Users.AnyAsync(u =>
                u.Id == request.LecturerId.Value && u.Role == UserRole.Lecturer && u.IsActive);
            if (!lecturerExists)
                throw new KeyNotFoundException($"Lecturer {request.LecturerId} not found or is inactive.");
            course.LecturerId = request.LecturerId.Value;
        }

        if (request.Title is not null) course.Title = request.Title.Trim();
        if (request.Description is not null) course.Description = request.Description.Trim();
        if (request.IsArchived.HasValue) course.IsArchived = request.IsArchived.Value;

        await _db.SaveChangesAsync();

        await _audit.LogAsync("Update", "Course", course.Id.ToString(), _currentUser.UserId,
            _currentUser.Role.ToString(), null, $"Course '{course.Title}' updated", null, null);

        return await GetByIdAsync(course.Id);
    }

    /// <inheritdoc />
    public Task ArchiveCourseAsync(Guid id)
        => throw new NotImplementedException("TODO: Set IsArchived = true. Archived courses are read-only for students.");

    /// <inheritdoc />
    public Task<List<UserDto>> GetEnrolledStudentsAsync(Guid courseId)
        => throw new NotImplementedException("TODO: Query Enrollments where CourseId = id and Status = Active.");

    private IQueryable<Course> BaseCourseQuery()
        => _db.Courses
            .AsNoTracking()
            .Include(c => c.Lecturer)
            .Include(c => c.Semester)
            .Include(c => c.Enrollments);

    private static IQueryable<Course> ApplyFilters(
        IQueryable<Course> courses,
        CourseQueryParams query,
        Guid requestingUserId,
        string role)
    {
        courses = ApplyRoleFilter(courses, requestingUserId, role);

        if (!string.IsNullOrWhiteSpace(query.Search))
            courses = ApplySearchFilter(courses, query.Search);

        if (query.SemesterId.HasValue)
            courses = courses.Where(c => c.SemesterId == query.SemesterId);

        return query.IsArchived.HasValue
            ? courses.Where(c => c.IsArchived == query.IsArchived)
            : courses;
    }

    private static IQueryable<Course> ApplyRoleFilter(
        IQueryable<Course> courses,
        Guid requestingUserId,
        string role)
        => role switch
        {
            nameof(UserRole.Student) => courses.Where(c => c.Enrollments.Any(e =>
                e.StudentId == requestingUserId && e.Status == EnrollmentStatus.Active)),
            nameof(UserRole.Lecturer) => courses.Where(c => c.LecturerId == requestingUserId),
            nameof(UserRole.Admin) => courses,
            _ => throw new UnauthorizedAccessException("Unsupported user role.")
        };

    private static IQueryable<Course> ApplySearchFilter(IQueryable<Course> courses, string search)
    {
        var term = $"%{search.Trim()}%";
        return courses.Where(c =>
            EF.Functions.ILike(c.Code, term) ||
            EF.Functions.ILike(c.Title, term));
    }

    private async Task ValidateCourseReferencesAsync(CreateCourseRequest request)
    {
        var lecturerExists = await _db.Users.AnyAsync(u =>
            u.Id == request.LecturerId && u.Role == UserRole.Lecturer && u.IsActive);

        if (!lecturerExists)
            throw new KeyNotFoundException($"Lecturer {request.LecturerId} not found.");

        var semesterExists = await _db.Semesters.AnyAsync(s => s.Id == request.SemesterId);
        if (!semesterExists)
            throw new KeyNotFoundException($"Semester {request.SemesterId} not found.");
    }

    private static void ValidateCreateRequest(CreateCourseRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
            throw new ArgumentException("Course code is required.", nameof(request));

        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ArgumentException("Course title is required.", nameof(request));
    }

    private static CourseDto MapToDto(Course course) => new()
    {
        Id = course.Id,
        Code = course.Code,
        Title = course.Title,
        Description = course.Description,
        CreditHours = course.CreditHours,
        IsArchived = course.IsArchived,
        LecturerName = $"{course.Lecturer.FirstName} {course.Lecturer.LastName}",
        SemesterName = course.Semester.Name,
        EnrolledStudentCount = course.Enrollments.Count(e => e.Status == EnrollmentStatus.Active)
    };
}
