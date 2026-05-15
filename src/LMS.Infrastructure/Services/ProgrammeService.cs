using LMS.Infrastructure.Data;
using LMS.Infrastructure.DTOs;
using LMS.Infrastructure.Entities;
using LMS.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Services;

/// <summary>
/// Implements degree programme management.
/// Pattern: constructor-inject AppDbContext, async/await throughout,
/// throw typed exceptions — ExceptionHandlingMiddleware maps them to HTTP status codes.
/// </summary>
public class ProgrammeService : IProgrammeService
{
    private readonly AppDbContext _db;

    public ProgrammeService(AppDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ProgrammeListDto>> GetAllAsync()
    {
        return await _db.Programmes
            .AsNoTracking()
            .OrderBy(p => p.Code)
            .Select(p => new ProgrammeListDto
            {
                Id = p.Id,
                Code = p.Code,
                Title = p.Title,
                Department = p.Department,
                Year = p.Year,
                CourseCount = p.Courses.Count
            })
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<ProgrammeDto> GetByIdAsync(Guid id)
    {
        var programme = await _db.Programmes
            .AsNoTracking()
            .Include(p => p.Courses)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException($"Programme {id} not found.");

        return MapToDto(programme);
    }

    /// <inheritdoc />
    public async Task<ProgrammeDto> CreateAsync(CreateProgrammeRequest request)
    {
        ValidateRequest(request);
        await EnsureCodeIsUniqueAsync(request.Code);

        var programme = new Programme
        {
            Id = Guid.NewGuid(),
            Code = request.Code.Trim(),
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Department = request.Department?.Trim(),
            Year = request.Year,
            CreatedAt = DateTime.UtcNow
        };

        _db.Programmes.Add(programme);
        await _db.SaveChangesAsync();

        return await GetByIdAsync(programme.Id);
    }

    // ─── Private helpers ──────────────────────────────────────────────────────

    private async Task EnsureCodeIsUniqueAsync(string code)
    {
        var isDuplicate = await _db.Programmes
            .AnyAsync(p => EF.Functions.ILike(p.Code, code.Trim()));

        if (isDuplicate)
            throw new InvalidOperationException($"Programme code '{code.Trim()}' is already in use.");
    }

    private static void ValidateRequest(CreateProgrammeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
            throw new ArgumentException("Programme code is required.", nameof(request));

        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ArgumentException("Programme title is required.", nameof(request));

        if (request.Year < 1 || request.Year > 10)
            throw new ArgumentException("Programme year must be between 1 and 10.", nameof(request));
    }

    private static ProgrammeDto MapToDto(Programme programme) => new()
    {
        Id = programme.Id,
        Code = programme.Code,
        Title = programme.Title,
        Description = programme.Description,
        Department = programme.Department,
        Year = programme.Year,
        CourseCount = programme.Courses.Count,
        CreatedAt = programme.CreatedAt
    };
}
