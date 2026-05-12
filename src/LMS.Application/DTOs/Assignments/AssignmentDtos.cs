using System.ComponentModel.DataAnnotations;

namespace LMS.Application.DTOs.Assignments;

public class AssignmentDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime Deadline { get; set; }
    public bool IsDeadlinePassed { get; set; }
    public int MaxMarks { get; set; }
    public bool AllowResubmission { get; set; }
    public bool AllowLateSubmission { get; set; }
    public bool TurnitinEnabled { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public int SubmissionCount { get; set; }
}

public class CreateAssignmentRequest
{
    [Required] public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    [Required] public DateTime Deadline { get; set; }
    [Required] public Guid CourseId { get; set; }
    [Range(1, 1000)] public int MaxMarks { get; set; } = 100;
    public bool AllowResubmission { get; set; } = false;
    public bool AllowLateSubmission { get; set; } = false;
    public bool TurnitinEnabled { get; set; } = false;
}

public class UpdateAssignmentRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime? Deadline { get; set; }
    public bool? AllowResubmission { get; set; }
    public bool? AllowLateSubmission { get; set; }
}
