using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using LMS.Application.Interfaces;
using LMS.Domain.Entities;
using LMS.Domain.Enums;
using LMS.Infrastructure.Data;
using LMS.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LMS.Infrastructure.Services;

/// <summary>
/// Uploads files to Cloudinary, persists metadata in the Files table, and supports
/// access-controlled download (proxied via Cloudinary URL) and deletion.
/// FileRecord.FileName stores the Cloudinary public_id; FileRecord.Path stores the HTTPS URL.
/// </summary>
public class FileService : IFileService
{
    private readonly AppDbContext _db;
    private readonly Cloudinary _cloudinary;
    private readonly ILogger<FileService> _logger;
    private static readonly HttpClient _http = new();

    private static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/zip",
        "image/jpeg",
        "image/png",
        "text/plain",
    };

    private const long MaxFileSizeBytes = 50L * 1024 * 1024; // 50 MB

    public FileService(
        AppDbContext db,
        IOptions<CloudinarySettings> settings,
        ILogger<FileService> logger)
    {
        _db = db;
        _logger = logger;
        var cfg = settings.Value;
        var account = new Account(cfg.CloudName, cfg.ApiKey, cfg.ApiSecret);
        _cloudinary = new Cloudinary(account) { Api = { Secure = true } };
    }

    /// <summary>
    /// Validates, uploads to Cloudinary, saves a FileRecord, and returns the record ID.
    /// </summary>
    public async Task<Guid> UploadAsync(
        Stream fileStream,
        string originalName,
        string mimeType,
        string relatedEntity,
        Guid entityId,
        Guid uploadedById)
    {
        if (fileStream is null || (fileStream.CanSeek && fileStream.Length == 0))
            throw new InvalidOperationException("No file provided.");

        if (fileStream.CanSeek && fileStream.Length > MaxFileSizeBytes)
            throw new InvalidOperationException("File size exceeds the 50 MB limit.");

        if (!AllowedMimeTypes.Contains(mimeType))
            throw new InvalidOperationException($"File type '{mimeType}' is not permitted.");

        var publicId = $"lms/{relatedEntity.ToLowerInvariant()}/{Guid.NewGuid():N}";
        var uploadParams = new RawUploadParams
        {
            File = new FileDescription(originalName, fileStream),
            PublicId = publicId,
            Overwrite = false,
            UniqueFilename = false,
        };

        var result = await _cloudinary.UploadAsync(uploadParams);

        if (result.Error is not null)
        {
            _logger.LogError("Cloudinary upload failed: {Error}", result.Error.Message);
            throw new InvalidOperationException($"Upload failed: {result.Error.Message}");
        }

        var record = new FileRecord
        {
            Id = Guid.NewGuid(),
            FileName = result.PublicId,              // Cloudinary public_id — used for deletion
            OriginalName = SanitiseFilename(originalName),
            Path = result.SecureUrl.ToString(),      // HTTPS Cloudinary URL — served to clients
            SizeBytes = result.Bytes,
            MimeType = mimeType,
            RelatedEntity = relatedEntity.ToLowerInvariant(),
            EntityId = entityId,
            UploadedById = uploadedById,
            UploadedAt = DateTime.UtcNow,
        };

        _db.Files.Add(record);
        await _db.SaveChangesAsync();

        return record.Id;
    }

    /// <summary>
    /// Downloads the file from its Cloudinary URL and returns the stream.
    /// Only the uploader or an Admin may access.
    /// </summary>
    public async Task<(Stream stream, string fileName, string mimeType)> DownloadAsync(
        Guid fileId,
        Guid requestingUserId)
    {
        var record = await _db.Files.FindAsync(fileId)
            ?? throw new KeyNotFoundException($"File {fileId} not found.");

        await RequireAccessAsync(record, requestingUserId);

        var stream = await _http.GetStreamAsync(record.Path);
        return (stream, record.OriginalName, record.MimeType);
    }

    /// <summary>
    /// Deletes the file from Cloudinary and removes its metadata record.
    /// Only the uploader or an Admin may delete.
    /// </summary>
    public async Task DeleteAsync(Guid fileId, Guid requestingUserId)
    {
        var record = await _db.Files.FindAsync(fileId)
            ?? throw new KeyNotFoundException($"File {fileId} not found.");

        await RequireAccessAsync(record, requestingUserId);

        var deleteParams = new DeletionParams(record.FileName) { ResourceType = ResourceType.Raw };
        await _cloudinary.DestroyAsync(deleteParams);

        _db.Files.Remove(record);
        await _db.SaveChangesAsync();
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private async Task RequireAccessAsync(FileRecord record, Guid requestingUserId)
    {
        if (record.UploadedById == requestingUserId) return;

        var user = await _db.Users.FindAsync(requestingUserId)
            ?? throw new KeyNotFoundException($"User {requestingUserId} not found.");

        if (user.Role != UserRole.Admin)
            throw new UnauthorizedAccessException("You do not have permission to access this file.");
    }

    private static string SanitiseFilename(string originalName)
    {
        var name = Path.GetFileName(originalName);
        var invalid = Path.GetInvalidFileNameChars().Concat(new[] { '/', '\\' }).Distinct().ToArray();
        name = string.Concat(name.Split(invalid));
        while (name.Contains("..")) name = name.Replace("..", ".");
        return string.IsNullOrWhiteSpace(name) ? "file" : name;
    }
}
