using LMS.Application.Interfaces;
using LMS.Domain.Entities;
using LMS.Domain.Enums;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace LMS.Infrastructure.Services;

/// <summary>
/// Handles file storage, retrieval, and access-controlled download.
/// Files are stored on disk under a configurable base path (appsettings key: FileStorage:BasePath)
/// and are never served directly — only through authenticated download endpoints.
/// </summary>
public class FileService : IFileService
{
    private readonly AppDbContext _db;
    private readonly string _basePath;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".docx", ".pptx", ".jpg", ".jpeg", ".png",
        ".zip", ".py", ".java", ".cs", ".js", ".ts", ".mp4"
    };

    private static readonly HashSet<string> AllowedRelatedEntities = new(StringComparer.OrdinalIgnoreCase)
    {
        "courses", "assignments", "submissions", "profiles"
    };

    private const long MaxFileSizeBytes = 25L * 1024 * 1024; // 25 MB

    public FileService(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _basePath = config["FileStorage:BasePath"]
            ?? Path.Combine(AppContext.BaseDirectory, "uploads");
    }

    /// <summary>
    /// Validates the file extension and size, sanitises the filename, stores the file on disk under
    /// <c>/uploads/{relatedEntity}/</c>, and saves metadata to the Files table.
    /// Throws <see cref="ArgumentException"/> for a disallowed extension, a file that exceeds 25 MB,
    /// or an unrecognised <paramref name="relatedEntity"/>.
    /// Returns the new <see cref="FileRecord"/> identifier.
    /// </summary>
    public async Task<Guid> UploadAsync(
        Stream fileStream,
        string originalName,
        string mimeType,
        string relatedEntity,
        Guid entityId,
        Guid uploadedById)
    {
        ValidateRelatedEntity(relatedEntity);
        ValidateExtension(originalName);

        if (!await _db.Users.AnyAsync(u => u.Id == uploadedById))
            throw new ArgumentException($"User {uploadedById} does not exist.");

        var extension = Path.GetExtension(originalName).ToLowerInvariant();
        var sanitisedOriginal = SanitiseFilename(originalName);
        var storedName = $"{Guid.NewGuid()}{extension}";
        var directory = Path.Combine(_basePath, relatedEntity.ToLowerInvariant());

        Directory.CreateDirectory(directory);

        var fullPath = Path.Combine(directory, storedName);
        long sizeBytes;

        try
        {
            await using var fileOut = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
            sizeBytes = await CopyWithSizeLimitAsync(fileStream, fileOut);
        }
        catch
        {
            if (File.Exists(fullPath)) File.Delete(fullPath);
            throw;
        }

        var record = new FileRecord
        {
            Id = Guid.NewGuid(),
            FileName = storedName,
            OriginalName = sanitisedOriginal,
            Path = fullPath,
            SizeBytes = sizeBytes,
            MimeType = mimeType,
            RelatedEntity = relatedEntity.ToLowerInvariant(),
            EntityId = entityId,
            UploadedById = uploadedById,
            UploadedAt = DateTime.UtcNow
        };

        _db.Files.Add(record);
        await _db.SaveChangesAsync();

        return record.Id;
    }

    /// <summary>
    /// Returns a readable <see cref="Stream"/> for the requested file along with its original name
    /// and MIME type. Only the file's uploader or an Admin may download.
    /// Throws <see cref="KeyNotFoundException"/> if the file record or physical file does not exist.
    /// Throws <see cref="UnauthorizedAccessException"/> if the caller lacks permission.
    /// </summary>
    public async Task<(Stream stream, string fileName, string mimeType)> DownloadAsync(
        Guid fileId,
        Guid requestingUserId)
    {
        var record = await _db.Files.FindAsync(fileId)
            ?? throw new KeyNotFoundException($"File {fileId} not found.");

        await RequireAccessAsync(record, requestingUserId);

        if (!File.Exists(record.Path))
            throw new KeyNotFoundException($"Physical file for {fileId} is missing from storage.");

        var stream = new FileStream(record.Path, FileMode.Open, FileAccess.Read, FileShare.Read);
        return (stream, record.OriginalName, record.MimeType);
    }

    /// <summary>
    /// Removes the physical file from disk and deletes its metadata record.
    /// Only the file's uploader or an Admin may delete.
    /// Throws <see cref="KeyNotFoundException"/> if the file does not exist.
    /// Throws <see cref="UnauthorizedAccessException"/> if the caller lacks permission.
    /// </summary>
    public async Task DeleteAsync(Guid fileId, Guid requestingUserId)
    {
        var record = await _db.Files.FindAsync(fileId)
            ?? throw new KeyNotFoundException($"File {fileId} not found.");

        await RequireAccessAsync(record, requestingUserId);

        _db.Files.Remove(record);
        await _db.SaveChangesAsync();

        if (File.Exists(record.Path))
            File.Delete(record.Path);
    }

    // ─── Private helpers ──────────────────────────────────────────────────────

    private async Task RequireAccessAsync(FileRecord record, Guid requestingUserId)
    {
        if (record.UploadedById == requestingUserId)
            return;

        var user = await _db.Users.FindAsync(requestingUserId)
            ?? throw new KeyNotFoundException($"User {requestingUserId} not found.");

        if (user.Role != UserRole.Admin)
            throw new UnauthorizedAccessException("You do not have permission to access this file.");
    }

    private static void ValidateRelatedEntity(string relatedEntity)
    {
        if (!AllowedRelatedEntities.Contains(relatedEntity))
            throw new ArgumentException(
                $"relatedEntity '{relatedEntity}' is not valid. Must be one of: {string.Join(", ", AllowedRelatedEntities)}.");
    }

    private static void ValidateExtension(string originalName)
    {
        var ext = Path.GetExtension(originalName);
        if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
            throw new ArgumentException(
                $"File type '{ext}' is not allowed. Allowed: {string.Join(", ", AllowedExtensions)}.");
    }

    private static string SanitiseFilename(string originalName)
    {
        // Strip any directory component the caller may have included
        var name = Path.GetFileName(originalName);

        // Remove characters that are illegal on Windows/Linux or that enable path traversal
        var invalid = Path.GetInvalidFileNameChars()
            .Concat(new[] { '/', '\\' })
            .Distinct()
            .ToArray();

        name = string.Concat(name.Split(invalid));

        // Collapse remaining dot-sequences that could still look like traversal on unusual platforms
        while (name.Contains(".."))
            name = name.Replace("..", ".");

        return string.IsNullOrWhiteSpace(name) ? "file" : name;
    }

    // Streams from source to destination, enforcing the 25 MB cap.
    // Returns the total number of bytes written.
    private static async Task<long> CopyWithSizeLimitAsync(Stream source, Stream destination)
    {
        var buffer = new byte[81_920]; // 80 KB chunks
        long totalRead = 0;
        int bytesRead;

        while ((bytesRead = await source.ReadAsync(buffer)) > 0)
        {
            totalRead += bytesRead;
            if (totalRead > MaxFileSizeBytes)
                throw new ArgumentException("File size exceeds the 25 MB limit.");
            await destination.WriteAsync(buffer.AsMemory(0, bytesRead));
        }

        return totalRead;
    }
}
