namespace LMS.Domain.Enums;

/// <summary>
/// Status codes for a student's attendance record per session.
/// See Section 26.3 of system documentation.
/// </summary>
public enum AttendanceStatus
{
    Present = 0,
    Absent = 1,
    Late = 2,
    Excused = 3,
    NotTaken = 4
}
