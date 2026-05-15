using LMS.Domain.Entities;

namespace LMS.Infrastructure.Entities;

/// <summary>
/// Represents a degree programme (e.g. BSc Computer Science) that groups
/// related courses under a named academic programme.
/// Lives in Infrastructure because Domain and Application layers are read-only.
/// </summary>
public class Programme
{
    public Guid Id { get; set; }

    /// <summary>Short unique programme code, e.g. "BSC-CS".</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Full programme title, e.g. "BSc Computer Science".</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Optional description of the programme's scope and goals.</summary>
    public string? Description { get; set; }

    /// <summary>Owning department, e.g. "Computer Science".</summary>
    public string? Department { get; set; }

    /// <summary>Academic year level of the programme (e.g. 1, 2, 3).</summary>
    public int Year { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Courses (modules) linked to this programme via the nullable shadow FK ProgrammeId.</summary>
    public ICollection<Course> Courses { get; set; } = new List<Course>();
}
