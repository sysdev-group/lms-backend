using LMS.Application.DTOs.Attendance;
using LMS.Application.Interfaces;
using LMS.Domain.Entities;
using LMS.Domain.Enums;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Services;

/// <summary>
/// Records and retrieves attendance data. See Section 26.
/// </summary>
public class AttendanceService : IAttendanceService
{
    private const double AttendanceWarningThreshold = 80.0;

    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public AttendanceService(AppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Marks attendance for all students in a timetable session on a given date.
    /// Creates a new <see cref="AttendanceSession"/> if one does not exist for the date;
    /// replaces existing records otherwise (provided the session is not closed).
    /// Lecturers may only mark attendance for sessions they own.
    /// </summary>
    /// <exception cref="UnauthorizedAccessException">Caller is a Student or a Lecturer who does not own the session.</exception>
    /// <exception cref="KeyNotFoundException">The timetable session does not exist.</exception>
    /// <exception cref="InvalidOperationException">The attendance session is already closed.</exception>
    public async Task MarkAttendanceAsync(MarkAttendanceRequest request, Guid lecturerId)
    {
        var role = _currentUser.Role;
        var userId = _currentUser.UserId;

        if (role == UserRole.Student)
            throw new UnauthorizedAccessException("Students cannot mark attendance.");

        var timetableSession = await _db.TimetableSessions
            .FirstOrDefaultAsync(ts => ts.Id == request.TimetableSessionId)
            ?? throw new KeyNotFoundException($"Timetable session {request.TimetableSessionId} not found.");

        if (role == UserRole.Lecturer && timetableSession.LecturerId != userId)
            throw new UnauthorizedAccessException("You do not own this timetable session.");

        var existingSession = await _db.AttendanceSessions
            .Include(s => s.Records)
            .FirstOrDefaultAsync(s => s.TimetableSessionId == request.TimetableSessionId
                                   && s.Date == request.Date);

        if (existingSession is not null)
        {
            if (existingSession.IsClosed)
                throw new InvalidOperationException("This attendance session is closed and cannot be modified.");

            _db.AttendanceRecords.RemoveRange(existingSession.Records);
            AddRecords(existingSession.Id, request.Records, userId, role);
            existingSession.TakenAt = DateTime.UtcNow;
        }
        else
        {
            var session = new AttendanceSession
            {
                Id = Guid.NewGuid(),
                TimetableSessionId = request.TimetableSessionId,
                LecturerId = lecturerId,
                Date = request.Date,
                TakenAt = DateTime.UtcNow
            };

            _db.AttendanceSessions.Add(session);
            AddRecords(session.Id, request.Records, userId, role);
        }

        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Returns all attendance records for a specific attendance session.
    /// Lecturers may only view records for sessions they own.
    /// </summary>
    /// <exception cref="UnauthorizedAccessException">Caller is a Student or a Lecturer who does not own the session.</exception>
    /// <exception cref="KeyNotFoundException">The attendance session does not exist.</exception>
    public async Task<List<AttendanceRecordDto>> GetSessionRecordsAsync(Guid attendanceSessionId)
    {
        var role = _currentUser.Role;
        var userId = _currentUser.UserId;

        if (role == UserRole.Student)
            throw new UnauthorizedAccessException("Students cannot view session attendance records.");

        var session = await _db.AttendanceSessions
            .AsNoTracking()
            .Include(s => s.TimetableSession)
            .FirstOrDefaultAsync(s => s.Id == attendanceSessionId)
            ?? throw new KeyNotFoundException($"Attendance session {attendanceSessionId} not found.");

        if (role == UserRole.Lecturer && session.TimetableSession.LecturerId != userId)
            throw new UnauthorizedAccessException("You do not own this attendance session.");

        return await _db.AttendanceRecords
            .AsNoTracking()
            .Where(r => r.AttendanceSessionId == attendanceSessionId)
            .Select(r => new AttendanceRecordDto
            {
                StudentId = r.StudentId,
                StudentName = r.Student.FirstName + " " + r.Student.LastName,
                Status = r.Status.ToString(),
                Notes = r.Notes
            })
            .ToListAsync();
    }

    /// <summary>
    /// Returns an attendance summary per enrolled course for the specified student.
    /// Reports total sessions, attended sessions (Present or Late), attendance percentage,
    /// and whether the student is below the <see cref="AttendanceWarningThreshold"/>.
    /// Students may only access their own summary; Lecturers and Admins may access any.
    /// </summary>
    /// <exception cref="UnauthorizedAccessException">A Student requested another student's summary.</exception>
    public async Task<List<StudentAttendanceSummaryDto>> GetStudentSummaryAsync(Guid studentId)
    {
        var role = _currentUser.Role;
        var userId = _currentUser.UserId;

        if (role == UserRole.Student && userId != studentId)
            throw new UnauthorizedAccessException("You may only view your own attendance summary.");

        var enrollments = await _db.Enrollments
            .AsNoTracking()
            .Where(e => e.StudentId == studentId && e.Status == EnrollmentStatus.Active)
            .Select(e => new { e.CourseId, e.Course.Title })
            .ToListAsync();

        if (enrollments.Count == 0)
            return new List<StudentAttendanceSummaryDto>();

        var courseIds = enrollments.Select(e => e.CourseId).ToList();

        var sessionData = await _db.AttendanceSessions
            .AsNoTracking()
            .Where(s => courseIds.Contains(s.TimetableSession.CourseId))
            .Select(s => new
            {
                CourseId = s.TimetableSession.CourseId,
                IsAttended = s.Records.Any(r => r.StudentId == studentId
                    && (r.Status == AttendanceStatus.Present || r.Status == AttendanceStatus.Late))
            })
            .ToListAsync();

        var grouped = sessionData
            .GroupBy(x => x.CourseId)
            .ToDictionary(
                g => g.Key,
                g => new { Total = g.Count(), Attended = g.Count(x => x.IsAttended) });

        return enrollments.Select(e =>
        {
            var stats = grouped.TryGetValue(e.CourseId, out var s)
                ? s
                : new { Total = 0, Attended = 0 };

            var pct = stats.Total == 0
                ? 100m
                : Math.Round((decimal)stats.Attended / stats.Total * 100, 2);

            return new StudentAttendanceSummaryDto
            {
                CourseId = e.CourseId,
                CourseName = e.Title,
                TotalSessions = stats.Total,
                AttendedSessions = stats.Attended,
                AttendancePercentage = pct,
                BelowWarningThreshold = (double)pct < AttendanceWarningThreshold
            };
        }).ToList();
    }

    private void AddRecords(Guid attendanceSessionId, List<StudentAttendanceEntry> entries, Guid actorId, UserRole role)
    {
        foreach (var entry in entries)
        {
            _db.AttendanceRecords.Add(new AttendanceRecord
            {
                Id = Guid.NewGuid(),
                AttendanceSessionId = attendanceSessionId,
                StudentId = entry.StudentId,
                Status = entry.Status,
                Notes = entry.Notes,
                RecordedAt = DateTime.UtcNow,
                OverriddenById = role == UserRole.Admin ? actorId : null
            });
        }
    }
}
