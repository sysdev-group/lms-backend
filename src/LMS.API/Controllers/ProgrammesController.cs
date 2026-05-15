using LMS.Infrastructure.DTOs;
using LMS.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers;

/// <summary>
/// Degree programme management.
/// Programmes sit above courses in the academic hierarchy:
/// Programme (BSc Computer Science) → Course (Operating Systems).
/// </summary>
[Route("api/v1/programmes")]
[Authorize]
public class ProgrammesController : BaseController
{
    private readonly IProgrammeService _programmeService;

    public ProgrammesController(IProgrammeService programmeService)
    {
        _programmeService = programmeService;
    }

    /// <summary>Get all degree programmes, each with a count of linked courses.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _programmeService.GetAllAsync();
        return ApiOk(result);
    }

    /// <summary>Get a single programme by ID, including its linked course count.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _programmeService.GetByIdAsync(id);
        return ApiOk(result);
    }

    /// <summary>Create a new degree programme. Admin only.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateProgrammeRequest request)
    {
        var result = await _programmeService.CreateAsync(request);
        return ApiCreated(result);
    }
}
