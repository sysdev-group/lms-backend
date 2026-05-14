using LMS.Domain.Entities;
using LMS.Domain.Enums;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LMS.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await SeedSemesterAsync(db);
        await SeedUsersAsync(db);
        await SeedCoursesAsync(db);
        await SeedEnrollmentsAsync(db);
        await SeedAssignmentsAsync(db);
        await SeedTimetableSessionsAsync(db);
    }

    // ─── Semester ─────────────────────────────────────────────────────────────

    private static async Task SeedSemesterAsync(AppDbContext db)
    {
        const string semesterName = "Semester 1 2025/2026";
        if (await db.Semesters.AnyAsync(s => s.Name == semesterName))
            return;

        var now = DateTime.UtcNow;
        db.Semesters.Add(new Semester
        {
            Name = semesterName,
            AcademicYear = "2025/2026",
            StartDate = now.AddDays(-30),
            EndDate = now.AddDays(120),
            EnrollmentOpenDate = now.AddDays(-60),
            EnrollmentCloseDate = now.AddDays(-15),
            GradeSubmissionDeadline = now.AddDays(135),
            Status = "Active"
        });

        await db.SaveChangesAsync();
    }

    // ─── Users ────────────────────────────────────────────────────────────────

    private static async Task SeedUsersAsync(AppDbContext db)
    {
        var seedEmails = new[]
        {
            "admin@lms.com",
            "lecturer@lms.com",
            "student1@lms.com",
            "student2@lms.com"
        };

        var existing = await db.Users
            .Where(u => seedEmails.Contains(u.Email))
            .Select(u => u.Email)
            .ToListAsync();

        var toAdd = new List<User>();

        if (!existing.Contains("admin@lms.com"))
            toAdd.Add(new User
            {
                FirstName = "System",
                LastName = "Admin",
                Email = "admin@lms.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = UserRole.Admin
            });

        if (!existing.Contains("lecturer@lms.com"))
            toAdd.Add(new User
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "lecturer@lms.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Lecturer@123"),
                Role = UserRole.Lecturer
            });

        if (!existing.Contains("student1@lms.com"))
            toAdd.Add(new User
            {
                FirstName = "Alice",
                LastName = "Johnson",
                Email = "student1@lms.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Student@123"),
                Role = UserRole.Student
            });

        if (!existing.Contains("student2@lms.com"))
            toAdd.Add(new User
            {
                FirstName = "Bob",
                LastName = "Williams",
                Email = "student2@lms.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Student@123"),
                Role = UserRole.Student
            });

        if (toAdd.Count == 0) return;

        db.Users.AddRange(toAdd);
        await db.SaveChangesAsync();
    }

    // ─── Courses ──────────────────────────────────────────────────────────────

    private static async Task SeedCoursesAsync(AppDbContext db)
    {
        var seedCodes = new[] { "CS101", "CS201" };
        var existing = await db.Courses
            .Where(c => seedCodes.Contains(c.Code))
            .Select(c => c.Code)
            .ToListAsync();

        if (existing.Count == seedCodes.Length) return;

        var semester = await db.Semesters.FirstAsync(s => s.Name == "Semester 1 2025/2026");
        var lecturer = await db.Users.FirstAsync(u => u.Email == "lecturer@lms.com");

        var toAdd = new List<Course>();

        if (!existing.Contains("CS101"))
            toAdd.Add(new Course
            {
                Code = "CS101",
                Title = "Introduction to Programming",
                CreditHours = 3,
                LecturerId = lecturer.Id,
                SemesterId = semester.Id
            });

        if (!existing.Contains("CS201"))
            toAdd.Add(new Course
            {
                Code = "CS201",
                Title = "Data Structures",
                CreditHours = 3,
                LecturerId = lecturer.Id,
                SemesterId = semester.Id
            });

        if (toAdd.Count == 0) return;

        db.Courses.AddRange(toAdd);
        await db.SaveChangesAsync();
    }

    // ─── Enrollments ──────────────────────────────────────────────────────────

    private static async Task SeedEnrollmentsAsync(AppDbContext db)
    {
        var semester = await db.Semesters.FirstAsync(s => s.Name == "Semester 1 2025/2026");
        var admin = await db.Users.FirstAsync(u => u.Email == "admin@lms.com");
        var student1 = await db.Users.FirstAsync(u => u.Email == "student1@lms.com");
        var student2 = await db.Users.FirstAsync(u => u.Email == "student2@lms.com");
        var cs101 = await db.Courses.FirstAsync(c => c.Code == "CS101");
        var cs201 = await db.Courses.FirstAsync(c => c.Code == "CS201");

        var existing = await db.Enrollments
            .Where(e => e.SemesterId == semester.Id)
            .Select(e => new { e.StudentId, e.CourseId })
            .ToListAsync();

        var toAdd = new List<Enrollment>();

        void AddIfMissing(Guid studentId, Guid courseId)
        {
            if (!existing.Any(e => e.StudentId == studentId && e.CourseId == courseId))
                toAdd.Add(new Enrollment
                {
                    StudentId = studentId,
                    CourseId = courseId,
                    SemesterId = semester.Id,
                    EnrolledById = admin.Id,
                    Status = EnrollmentStatus.Active
                });
        }

        AddIfMissing(student1.Id, cs101.Id);
        AddIfMissing(student2.Id, cs101.Id);
        AddIfMissing(student1.Id, cs201.Id);

        if (toAdd.Count == 0) return;

        db.Enrollments.AddRange(toAdd);
        await db.SaveChangesAsync();
    }

    // ─── Assignments ──────────────────────────────────────────────────────────

    private static async Task SeedAssignmentsAsync(AppDbContext db)
    {
        var lecturer = await db.Users.FirstAsync(u => u.Email == "lecturer@lms.com");
        var cs101 = await db.Courses.FirstAsync(c => c.Code == "CS101");
        var cs201 = await db.Courses.FirstAsync(c => c.Code == "CS201");

        var existing = await db.Assignments
            .Where(a => a.CourseId == cs101.Id || a.CourseId == cs201.Id)
            .Select(a => new { a.CourseId, a.Title })
            .ToListAsync();

        var toAdd = new List<Assignment>();
        var now = DateTime.UtcNow;

        if (!existing.Any(a => a.CourseId == cs101.Id && a.Title == "Hello World Project"))
            toAdd.Add(new Assignment
            {
                Title = "Hello World Project",
                CourseId = cs101.Id,
                CreatedById = lecturer.Id,
                Deadline = now.AddDays(7),
                MaxMarks = 100
            });

        if (!existing.Any(a => a.CourseId == cs201.Id && a.Title == "Linked List Implementation"))
            toAdd.Add(new Assignment
            {
                Title = "Linked List Implementation",
                CourseId = cs201.Id,
                CreatedById = lecturer.Id,
                Deadline = now.AddDays(14),
                MaxMarks = 100
            });

        if (toAdd.Count == 0) return;

        db.Assignments.AddRange(toAdd);
        await db.SaveChangesAsync();
    }

    // ─── Timetable Sessions ───────────────────────────────────────────────────

    private static async Task SeedTimetableSessionsAsync(AppDbContext db)
    {
        var semester = await db.Semesters.FirstAsync(s => s.Name == "Semester 1 2025/2026");
        var lecturer = await db.Users.FirstAsync(u => u.Email == "lecturer@lms.com");
        var cs101 = await db.Courses.FirstAsync(c => c.Code == "CS101");
        var cs201 = await db.Courses.FirstAsync(c => c.Code == "CS201");

        var existing = await db.TimetableSessions
            .Where(t => t.SemesterId == semester.Id && (t.CourseId == cs101.Id || t.CourseId == cs201.Id))
            .Select(t => new { t.CourseId, t.DayOfWeek })
            .ToListAsync();

        var toAdd = new List<TimetableSession>();

        if (!existing.Any(t => t.CourseId == cs101.Id && t.DayOfWeek == DayOfWeek.Monday))
            toAdd.Add(new TimetableSession
            {
                CourseId = cs101.Id,
                LecturerId = lecturer.Id,
                SemesterId = semester.Id,
                DayOfWeek = DayOfWeek.Monday,
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(11, 0),
                Room = "Lab A",
                Type = "Lab",
                IsPublished = true
            });

        if (!existing.Any(t => t.CourseId == cs201.Id && t.DayOfWeek == DayOfWeek.Wednesday))
            toAdd.Add(new TimetableSession
            {
                CourseId = cs201.Id,
                LecturerId = lecturer.Id,
                SemesterId = semester.Id,
                DayOfWeek = DayOfWeek.Wednesday,
                StartTime = new TimeOnly(13, 0),
                EndTime = new TimeOnly(15, 0),
                Room = "Lab B",
                Type = "Lab",
                IsPublished = true
            });

        if (toAdd.Count == 0) return;

        db.TimetableSessions.AddRange(toAdd);
        await db.SaveChangesAsync();
    }
}
