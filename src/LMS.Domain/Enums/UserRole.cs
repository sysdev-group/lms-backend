namespace LMS.Domain.Enums;

/// <summary>
/// Defines the roles available in the system.
/// Controls access level via RBAC — see AuthorizationModule in docs (Section 7.2).
/// </summary>
public enum UserRole
{
    Student = 0,
    Lecturer = 1,
    Admin = 2
}
