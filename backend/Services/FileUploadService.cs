using Microsoft.EntityFrameworkCore;
using ProjectManagement.Data;
using ProjectManagement.DTOs;
using ProjectManagement.Models;

namespace ProjectManagement.Services;

/// <summary>
/// Handles secure file uploads for project attachments.
/// Files are stored under  wwwroot/uploads/{userId}/{guid}{ext}
/// </summary>
public class FileUploadService : IFileUploadService
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx", ".xls", ".xlsx",
        ".png", ".jpg", ".jpeg", ".gif",
        ".txt", ".csv", ".zip"
    };

    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<FileUploadService> _logger;

    public FileUploadService(AppDbContext context, IWebHostEnvironment env, ILogger<FileUploadService> logger)
    {
        _context = context;
        _env     = env;
        _logger  = logger;
    }

    public async Task<AttachmentResponseDto> UploadAsync(IFormFile file, int projectId, int userId)
    {
        if (file.Length == 0)
            throw new ArgumentException("File is empty");

        if (file.Length > MaxFileSizeBytes)
            throw new ArgumentException($"File exceeds max size of {MaxFileSizeBytes / 1024 / 1024} MB");

        var ext = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(ext))
            throw new ArgumentException($"File type '{ext}' is not allowed");
        var projectExists = await _context.Projects
            .AnyAsync(p => p.Id == projectId && p.UserId == userId);

        if (!projectExists)
            throw new UnauthorizedAccessException("Project not found or access denied");
        var uploadRoot = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", userId.ToString());
        Directory.CreateDirectory(uploadRoot);

        var storedName = $"{Guid.NewGuid()}{ext}";
        var filePath   = Path.Combine(uploadRoot, storedName);

        await using (var stream = File.Create(filePath))
            await file.CopyToAsync(stream);
        var attachment = new ProjectAttachment
        {
            ProjectId        = projectId,
            FileName         = Path.GetFileName(file.FileName), // strip any path info
            StoredFileName   = storedName,
            ContentType      = file.ContentType,
            FileSize         = file.Length,
            UploadedByUserId = userId,
            UploadedAt       = DateTime.UtcNow
        };

        _context.ProjectAttachments.Add(attachment);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} uploaded {FileName} to project {ProjectId}", userId, file.FileName, projectId);
        return ToDto(attachment);
    }

    public async Task<IEnumerable<AttachmentResponseDto>> GetByProjectAsync(int projectId, int userId)
    {
        // Verify ownership
        var projectExists = await _context.Projects
            .AnyAsync(p => p.Id == projectId && p.UserId == userId);

        if (!projectExists)
            throw new UnauthorizedAccessException("Project not found or access denied");

        var attachments = await _context.ProjectAttachments
            .Where(a => a.ProjectId == projectId)
            .OrderByDescending(a => a.UploadedAt)
            .ToListAsync();

        return attachments.Select(ToDto);
    }

    public async Task<bool> DeleteAsync(int attachmentId, int userId)
    {
        var attachment = await _context.ProjectAttachments
            .Include(a => a.Project)
            .FirstOrDefaultAsync(a => a.Id == attachmentId && a.Project.UserId == userId);

        if (attachment is null) return false;

        // Remove from disk
        var filePath = Path.Combine(
            _env.WebRootPath ?? "wwwroot",
            "uploads",
            userId.ToString(),
            attachment.StoredFileName);

        if (File.Exists(filePath))
            File.Delete(filePath);

        _context.ProjectAttachments.Remove(attachment);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} deleted attachment {AttachmentId}", userId, attachmentId);
        return true;
    }

    public async Task<(byte[] Data, string ContentType, string FileName)> DownloadAsync(int attachmentId, int userId)
    {
        var attachment = await _context.ProjectAttachments
            .Include(a => a.Project)
            .FirstOrDefaultAsync(a => a.Id == attachmentId && a.Project.UserId == userId);

        if (attachment is null)
            throw new FileNotFoundException("Attachment not found or access denied");

        var filePath = Path.Combine(
            _env.WebRootPath ?? "wwwroot",
            "uploads",
            userId.ToString(),
            attachment.StoredFileName);

        if (!File.Exists(filePath))
            throw new FileNotFoundException("File not found on server");

        var data = await File.ReadAllBytesAsync(filePath);
        return (data, attachment.ContentType, attachment.FileName);
    }

    private static AttachmentResponseDto ToDto(ProjectAttachment a) => new(
        a.Id,
        a.ProjectId,
        a.FileName,
        a.ContentType,
        a.FileSize,
        a.UploadedByUserId,
        a.UploadedAt
    );
}
