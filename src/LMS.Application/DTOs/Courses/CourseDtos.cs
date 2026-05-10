using System.ComponentModel.DataAnnotations;

namespace LMS.Application.DTOs.Courses;

public class CourseDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CreditHours { get; set; }
    public bool IsArchived { get; set; }
    public string LecturerName { get; set; } = string.Empty;
    public string SemesterName { get; set; } = string.Empty;
    public int EnrolledStudentCount { get; set; }
}

public class CreateCourseRequest
{
    [Required] public string Code { get; set; } = string.Empty;
    [Required] public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    [Required] public Guid LecturerId { get; set; }
    [Required] public Guid SemesterId { get; set; }
    [Range(1, 12)] public int CreditHours { get; set; }
}

public class UpdateCourseRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public Guid? LecturerId { get; set; }
    public bool? IsArchived { get; set; }
}

public class CourseQueryParams
{
    public string? Search { get; set; }
    public Guid? SemesterId { get; set; }
    public bool? IsArchived { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
