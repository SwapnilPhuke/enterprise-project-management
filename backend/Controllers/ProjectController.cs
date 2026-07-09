
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using Asp.Versioning;
using ProjectManagement.Common;
using ProjectManagement.Hubs;
using ProjectManagement.Services;
using ProjectManagement.DTOs;

namespace ProjectManagement.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/projects")]
[Authorize]
public class ProjectController : ControllerBase
{
    private readonly IProjectService            _service;
    private readonly ILogger<ProjectController> _logger;
    private readonly IHubContext<ProjectStatusHub> _hub;

    public ProjectController(
        IProjectService service,
        ILogger<ProjectController> logger,
        IHubContext<ProjectStatusHub> hub)
    {
        _service = service;
        _logger  = logger;
        _hub     = hub;
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (!int.TryParse(claim?.Value, out var userId))
            throw new UnauthorizedAccessException("Invalid user token");
        return userId;
    }

    private IActionResult ToActionResult<T>(ServiceResult<T> result) => result.StatusCode switch
    {
        200 => Ok(result.Data),
        404 => NotFound(new { message = result.ErrorMessage }),
        400 => BadRequest(new { message = result.ErrorMessage }),
        _   => StatusCode(result.StatusCode, new { message = result.ErrorMessage })
    };

    /// <summary>Get paginated + filtered projects for the authenticated user.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll([FromQuery] ProjectQueryParams query)
    {
        var userId = GetCurrentUserId();
        var result = await _service.GetPagedAsync(userId, query);
        _logger.LogInformation("User {UserId} retrieved page {Page}", userId, query.Page);
        return Ok(result.Data);
    }

    /// <summary>Get project portfolio statistics for the authenticated user.</summary>
    [HttpGet("stats")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetStats()
    {
        var userId = GetCurrentUserId();
        var result = await _service.GetStatsAsync(userId);
        return Ok(result.Data);
    }

    /// <summary>Get a project by ID (must belong to the authenticated user).</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = GetCurrentUserId();
        var result = await _service.GetByIdAsync(id, userId);
        return ToActionResult(result);
    }

    /// <summary>Create a new project for the authenticated user.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] ProjectDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = GetCurrentUserId();
        var result = await _service.CreateAsync(dto, userId);
        _logger.LogInformation("User {UserId} created project {ProjectId}", userId, result.Data!.Id);
        return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result.Data);
    }

    /// <summary>Update a project (must belong to the authenticated user).</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] ProjectUpdateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = GetCurrentUserId();
        var result = await _service.UpdateAsync(id, dto, userId);
        if (result.Success)
        {
            _logger.LogInformation("User {UserId} updated project {ProjectId}", userId, id);

            // Broadcast real-time status update to all clients watching this project
            await _hub.Clients
                .Group(ProjectStatusHub.GetGroupName(id))
                .SendAsync("ProjectStatusChanged", new
                {
                    projectId = id,
                    status    = result.Data!.Status,
                    updatedAt = result.Data.UpdatedAt
                });
        }
        return ToActionResult(result);
    }

    /// <summary>Delete a project (must belong to the authenticated user).</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetCurrentUserId();
        var result = await _service.DeleteAsync(id, userId);
        if (!result.Success)
            return NotFound(new { message = result.ErrorMessage });

        _logger.LogInformation("User {UserId} deleted project {ProjectId}", userId, id);
        return NoContent();
    }
}

