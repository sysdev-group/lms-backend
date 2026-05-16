using LMS.Application.Interfaces;
using LMS.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.API.Controllers;

/// <summary>File upload and metadata endpoints.</summary>
[Route("api/v1/files")]
[Authorize]
public class FilesController : BaseController
{
    private readonly IFileService _fileService;
    private readonly AppDbContext _db;

    public FilesController(IFileService fileService, AppDbContext db)
    {
        _fileService = fileService;
        _db = db;
    }

    /// <summary>Upload a file to Cloudinary. Returns metadata including the Cloudinary URL.</summary>
    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] IFormFile file)
    {
        var id = await _fileService.UploadAsync(
            file.OpenReadStream(), file.FileName, file.ContentType,
            "submissions", Guid.Empty, CurrentUserId);
        var r = await _db.Files.AsNoTracking().FirstOrDefaultAsync(f => f.Id == id);
        return ApiCreated(new { fileId = r!.Id, fileName = r.OriginalName, fileUrl = r.Path, fileSizeBytes = r.SizeBytes, contentType = r.MimeType });
    }

    /// <summary>Get file metadata by ID (includes the Cloudinary URL for direct access).</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var r = await _db.Files.AsNoTracking().FirstOrDefaultAsync(f => f.Id == id)
            ?? throw new KeyNotFoundException($"File {id} not found.");
        return ApiOk(new { fileId = r.Id, fileName = r.OriginalName, fileUrl = r.Path, fileSizeBytes = r.SizeBytes, contentType = r.MimeType });
    }
}
