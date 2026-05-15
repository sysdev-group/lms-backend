using System.ComponentModel.DataAnnotations;

namespace LMS.Infrastructure.DTOs;

/// <summary>Full programme detail — returned by GetByIdAsync.</summary>
public class ProgrammeDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Department { get; set; }
    public int Year { get; set; }
    public int CourseCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>Lightweight programme summary — returned by GetAllAsync list views.</summary>
public class ProgrammeListDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Department { get; set; }
    public int Year { get; set; }
    public int CourseCount { get; set; }
}

/// <summary>Request body for creating a new degree programme.</summary>
public class CreateProgrammeRequest
{
    [Required]
    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(200)]
    public string? Department { get; set; }

    [Required]
    [Range(1, 10)]
    public int Year { get; set; }
}
