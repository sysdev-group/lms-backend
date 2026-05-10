namespace LMS.Domain.Enums;

/// <summary>
/// Lifecycle status of a student enrollment record.
/// Dropped enrollments are soft-deleted — history is retained. See Section 28.
/// </summary>
public enum EnrollmentStatus
{
    Active = 0,
    Dropped = 1,
    Completed = 2
}
