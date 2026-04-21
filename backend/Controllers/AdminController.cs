using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Asp.Versioning;
using ProjectManagement.Data;

namespace ProjectManagement.Controllers;

/// <summary>
/// Admin-only endpoints. Requires the "Admin" role claim in the JWT.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<AdminController> _logger;

    public AdminController(AppDbContext context, ILogger<AdminController> logger)
    {
        _context = context;
        _logger  = logger;
    }

    /// <summary>List all users (paginated).</summary>
    [HttpGet("users")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int page     = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _context.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(u =>
                u.Username.ToLower().Contains(term) ||
                u.Email.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync();
        var users = await query
            .OrderBy(u => u.Username)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new
            {
                u.Id,
                u.Username,
                u.Email,
                u.FullName,
                u.Role,
                u.IsActive,
                u.CreatedAt,
                u.LastLogin,
                ProjectCount = u.Projects.Count
            })
            .ToListAsync();

        return Ok(new
        {
            items      = users,
            totalCount,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        });
    }

    /// <summary>Toggle a user's IsActive flag (enable / disable account).</summary>
    [HttpPut("users/{id:int}/toggle-active")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user is null)
            return NotFound(new { message = "User not found" });

        user.IsActive = !user.IsActive;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Admin toggled IsActive={Active} for user {UserId}", user.IsActive, id);
        return Ok(new { id = user.Id, isActive = user.IsActive });
    }

    /// <summary>Promote or demote a user's role ("User" | "Admin").</summary>
    [HttpPut("users/{id:int}/role")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetRole(int id, [FromBody] SetRoleRequest request)
    {
        if (request.Role is not ("User" or "Admin"))
            return BadRequest(new { message = "Role must be 'User' or 'Admin'" });

        var user = await _context.Users.FindAsync(id);
        if (user is null)
            return NotFound(new { message = "User not found" });

        user.Role = request.Role;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Admin set role={Role} for user {UserId}", request.Role, id);
        return Ok(new { id = user.Id, role = user.Role });
    }
}

public record SetRoleRequest([System.ComponentModel.DataAnnotations.Required] string Role);
