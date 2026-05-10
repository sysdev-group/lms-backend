namespace LMS.Domain.Enums;

/// <summary>
/// Represents the lifecycle state of a student's assignment submission.
/// </summary>
public enum SubmissionStatus
{
    Draft = 0,
    Submitted = 1,
    Late = 2,
    Graded = 3,
    Resubmitted = 4
}
