using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Asp.Versioning;
using ProjectManagement.Services;

namespace ProjectManagement.Controllers;

/// <summary>
/// Upload, list, download and delete file attachments on a project.
/// All endpoints require the user to own the project.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/projects/{projectId:int}/attachments")]
[Authorize]
public class AttachmentController : ControllerBase
{
    private readonly IFileUploadService _fileService;
    private readonly ILogger<AttachmentController> _logger;

    public AttachmentController(IFileUploadService fileService, ILogger<AttachmentController> logger)
    {
        _fileService = fileService;
        _logger      = logger;
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (!int.TryParse(claim?.Value, out var uid))
            throw new UnauthorizedAccessException("Invalid user token");
        return uid;
    }

    /// <summary>List all attachments for a project.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> List(int projectId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var items  = await _fileService.GetByProjectAsync(projectId, userId);
            return Ok(items);
        }
        catch (UnauthorizedAccessException)
        {
            return NotFound(new { message = "Project not found or access denied" });
        }
    }

    /// <summary>Upload a file (max 10 MB; pdf/doc/docx/xlsx/png/jpg/gif/txt/csv/zip).</summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Upload(int projectId, IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "No file provided" });

        try
        {
            var userId = GetCurrentUserId();
            var result = await _fileService.UploadAsync(file, projectId, userId);
            _logger.LogInformation("Attachment {Id} created for project {ProjectId}", result.Id, projectId);
            return CreatedAtAction(nameof(List), new { projectId }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return NotFound(new { message = "Project not found or access denied" });
        }
    }

    /// <summary>Download an attachment by ID.</summary>
    [HttpGet("{attachmentId:int}/download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Download(int projectId, int attachmentId)
    {
        try
        {
            var userId         = GetCurrentUserId();
            var (data, ct, fn) = await _fileService.DownloadAsync(attachmentId, userId);
            return File(data, ct, fn);
        }
        catch (FileNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>Delete an attachment.</summary>
    [HttpDelete("{attachmentId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int projectId, int attachmentId)
    {
        var userId  = GetCurrentUserId();
        var deleted = await _fileService.DeleteAsync(attachmentId, userId);
        return deleted ? NoContent() : NotFound(new { message = "Attachment not found" });
    }
}
