using LMS.Infrastructure.DTOs;

namespace LMS.Infrastructure.Interfaces;

/// <summary>Manages degree programmes in the LMS hierarchy above courses.</summary>
public interface IProgrammeService
{
    /// <summary>Returns all programmes ordered by code, each with a course count.</summary>
    Task<IEnumerable<ProgrammeListDto>> GetAllAsync();

    /// <summary>
    /// Returns a single programme by ID, including its linked course count.
    /// Throws <see cref="KeyNotFoundException"/> if the programme does not exist.
    /// </summary>
    Task<ProgrammeDto> GetByIdAsync(Guid id);

    /// <summary>
    /// Creates a new degree programme.
    /// Throws <see cref="InvalidOperationException"/> if the code is already in use.
    /// Throws <see cref="ArgumentException"/> if required fields are missing or invalid.
    /// </summary>
    Task<ProgrammeDto> CreateAsync(CreateProgrammeRequest request);
}
