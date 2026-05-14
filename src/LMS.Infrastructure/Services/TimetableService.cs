using LMS.Application.DTOs.Timetable;
using LMS.Application.Interfaces;
using LMS.Domain.Entities;
using LMS.Domain.Enums;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Services;

/// <summary>
/// Manages timetable sessions and conflict detection. See Section 24.
/// </summary>
public class TimetableService : ITimetableService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public TimetableService(AppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Returns timetable sessions for a semester, filtered by the caller's role.
    /// Students see only published sessions for courses they are actively enrolled in.
    /// Lecturers see all sessions (published or not) for their own courses, optionally scoped to a specific lecturer.
    /// Admins see all sessions across all courses.
    /// </summary>
    public async Task<List<TimetableSessionDto>> GetByBatchAsync(Guid semesterId, Guid? lecturerId = null)
    {
        var userId = _currentUser.UserId;
        var role = _currentUser.Role;

        var query = _db.TimetableSessions
            .Where(ts => ts.SemesterId == semesterId)
            .AsQueryable();

        if (role == UserRole.Student)
        {
            var enrolledCourseIds = await _db.Enrollments
                .Where(e => e.StudentId == userId && e.SemesterId == semesterId && e.Status == EnrollmentStatus.Active)
                .Select(e => e.CourseId)
                .ToListAsync();

            query = query.Where(ts => ts.IsPublished && enrolledCourseIds.Contains(ts.CourseId));
        }
        else if (role == UserRole.Lecturer)
        {
            query = query.Where(ts => ts.LecturerId == userId);
        }

        if (lecturerId.HasValue && role != UserRole.Student)
            query = query.Where(ts => ts.LecturerId == lecturerId.Value);

        return await query
            .Select(ts => new TimetableSessionDto
            {
                Id = ts.Id,
                CourseTitle = ts.Course.Title,
                LecturerName = ts.Lecturer.FirstName + " " + ts.Lecturer.LastName,
                Room = ts.Room,
                DayOfWeek = ts.DayOfWeek.ToString(),
                StartTime = ts.StartTime.ToString("HH:mm"),
                EndTime = ts.EndTime.ToString("HH:mm"),
                Type = ts.Type,
                IsPublished = ts.IsPublished
            })
            .ToListAsync();
    }

    /// <summary>
    /// Creates a new timetable session after running conflict detection.
    /// Checks for room conflicts (same room, day, and overlapping time) and
    /// lecturer conflicts (same lecturer, day, and overlapping time).
    /// Lecturers may only create sessions for their own courses.
    /// Throws <see cref="UnauthorizedAccessException"/> if the lecturer does not own the course.
    /// Throws <see cref="KeyNotFoundException"/> if the course is not found.
    /// Throws <see cref="InvalidOperationException"/> if a scheduling conflict is detected.
    /// </summary>
    public async Task<TimetableSessionDto> CreateSessionAsync(CreateSessionRequest request)
    {
        var role = _currentUser.Role;
        var userId = _currentUser.UserId;

        if (role == UserRole.Student)
            throw new UnauthorizedAccessException("Students cannot create timetable sessions.");

        var course = await _db.Courses.FindAsync(request.CourseId)
            ?? throw new KeyNotFoundException($"Course {request.CourseId} not found.");

        if (role == UserRole.Lecturer && course.LecturerId != userId)
            throw new UnauthorizedAccessException("You do not own this course.");

        await EnsureNoConflictsAsync(request.SemesterId, request.DayOfWeek, request.StartTime, request.EndTime,
            request.Room, request.LecturerId, excludeSessionId: null);

        var session = new TimetableSession
        {
            Id = Guid.NewGuid(),
            CourseId = request.CourseId,
            LecturerId = request.LecturerId,
            SemesterId = request.SemesterId,
            DayOfWeek = request.DayOfWeek,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Room = request.Room,
            Type = request.Type,
            IsPublished = false,
            CreatedAt = DateTime.UtcNow
        };

        _db.TimetableSessions.Add(session);
        await _db.SaveChangesAsync();

        var lecturer = await _db.Users.FindAsync(request.LecturerId);
        return new TimetableSessionDto
        {
            Id = session.Id,
            CourseTitle = course.Title,
            LecturerName = lecturer is null ? string.Empty : $"{lecturer.FirstName} {lecturer.LastName}",
            Room = session.Room,
            DayOfWeek = session.DayOfWeek.ToString(),
            StartTime = session.StartTime.ToString("HH:mm"),
            EndTime = session.EndTime.ToString("HH:mm"),
            Type = session.Type,
            IsPublished = session.IsPublished
        };
    }

    /// <summary>
    /// Sets <c>IsPublished = true</c> on a timetable session, making it visible to enrolled students.
    /// Lecturers may only publish sessions for their own courses.
    /// Throws <see cref="UnauthorizedAccessException"/> if the caller does not own the session.
    /// Throws <see cref="KeyNotFoundException"/> if the session does not exist.
    /// </summary>
    public async Task PublishSessionAsync(Guid sessionId)
    {
        var session = await _db.TimetableSessions.FindAsync(sessionId)
            ?? throw new KeyNotFoundException($"Timetable session {sessionId} not found.");

        EnsureManageAccess(session);

        if (session.IsPublished)
            return;

        session.IsPublished = true;
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Deletes a timetable session.
    /// Only unpublished sessions may be deleted; published sessions cannot be removed to preserve attendance history.
    /// Lecturers may only delete sessions for their own courses.
    /// Throws <see cref="UnauthorizedAccessException"/> if the caller does not own the session.
    /// Throws <see cref="KeyNotFoundException"/> if the session does not exist.
    /// Throws <see cref="InvalidOperationException"/> if the session has already been published.
    /// </summary>
    public async Task DeleteSessionAsync(Guid sessionId)
    {
        var session = await _db.TimetableSessions.FindAsync(sessionId)
            ?? throw new KeyNotFoundException($"Timetable session {sessionId} not found.");

        EnsureManageAccess(session);

        if (session.IsPublished)
            throw new InvalidOperationException("Cannot delete a published timetable session.");

        _db.TimetableSessions.Remove(session);
        await _db.SaveChangesAsync();
    }

    // ─── Private helpers ──────────────────────────────────────────────────────

    private void EnsureManageAccess(TimetableSession session)
    {
        var role = _currentUser.Role;
        if (role == UserRole.Admin) return;
        if (role == UserRole.Student)
            throw new UnauthorizedAccessException("Students cannot manage timetable sessions.");
        if (session.LecturerId != _currentUser.UserId)
            throw new UnauthorizedAccessException("You do not own this timetable session.");
    }

    private async Task EnsureNoConflictsAsync(Guid semesterId, DayOfWeek day, TimeOnly start, TimeOnly end,
        string room, Guid lecturerId, Guid? excludeSessionId)
    {
        var overlapping = await _db.TimetableSessions
            .Where(ts => ts.SemesterId == semesterId
                      && ts.DayOfWeek == day
                      && (excludeSessionId == null || ts.Id != excludeSessionId)
                      && ts.StartTime < end
                      && ts.EndTime > start
                      && (ts.Room == room || ts.LecturerId == lecturerId))
            .AnyAsync();

        if (overlapping)
            throw new InvalidOperationException(
                "Scheduling conflict: the room or lecturer is already booked for an overlapping time slot.");
    }
}
