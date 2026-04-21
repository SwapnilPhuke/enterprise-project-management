using System.ComponentModel.DataAnnotations;

namespace ProjectManagement.Models;

/// <summary>File attachment associated with a project.</summary>
public class ProjectAttachment
{
    [Key]
    public int Id { get; set; }

    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    /// <summary>Original filename as uploaded by the user.</summary>
    [Required]
    [StringLength(260)]
    public required string FileName { get; set; }

    /// <summary>GUID-based name used when storing on disk — prevents path traversal.</summary>
    [Required]
    [StringLength(260)]
    public required string StoredFileName { get; set; }

    [Required]
    [StringLength(100)]
    public required string ContentType { get; set; }

    /// <summary>File size in bytes.</summary>
    public long FileSize { get; set; }

    public int UploadedByUserId { get; set; }
    public User UploadedByUser { get; set; } = null!;

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
