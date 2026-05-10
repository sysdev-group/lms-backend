namespace LMS.Domain.Enums;

/// <summary>
/// Priority level for notifications. Affects display prominence in the UI.
/// See Section 7.7 of system documentation.
/// </summary>
public enum NotificationPriority
{
    Normal = 0,
    Important = 1,
    Urgent = 2
}
