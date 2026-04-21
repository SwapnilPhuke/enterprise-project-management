using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Asp.Versioning;
using ProjectManagement.Services;
using ProjectManagement.DTOs;

namespace ProjectManagement.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthenticationService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>Register a new user account.</summary>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RegisterAsync(
                request.Username, request.Email, request.Password, request.FullName);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            _logger.LogInformation("User registered: {Username}", request.Username);
            return CreatedAtAction(nameof(Register), new { message = result.Message, userId = result.UserId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for {Username}", request.Username);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred during registration" });
        }
    }

    /// <summary>Authenticate and receive a JWT token.</summary>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.LoginAsync(request.Username, request.Password);

            if (!result.Success)
                return Unauthorized(new { message = result.Message });

            _logger.LogInformation("User logged in: {Username}", request.Username);

            return Ok(new
            {
                message      = result.Message,
                token        = result.Token,
                refreshToken = result.RefreshToken,
                userId       = result.UserId,
                username     = result.Username,
                role         = result.Role
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for {Username}", request.Username);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred during login" });
        }
    }

    /// <summary>Return the profile of the authenticated user.</summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim?.Value, out var userId))
                return Unauthorized(new { message = "Invalid user token" });

            var user = await _authService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound(new { message = "User not found" });

            return Ok(new
            {
                id        = user.Id,
                username  = user.Username,
                email     = user.Email,
                fullName  = user.FullName,
                role      = user.Role,
                createdAt = user.CreatedAt,
                lastLogin = user.LastLogin
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching current user");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred" });
        }
    }

    /// <summary>Issue a new access token using a valid refresh token.</summary>
    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return BadRequest(new { message = "Refresh token is required" });

        var result = await _authService.RefreshTokenAsync(request.RefreshToken);
        if (!result.Success)
            return Unauthorized(new { message = result.Message });

        return Ok(new
        {
            token        = result.Token,
            refreshToken = result.RefreshToken,
            role         = result.Role
        });
    }

    /// <summary>Revoke the current user's refresh tokens (logout).</summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (int.TryParse(userIdClaim?.Value, out var userId))
        {
            await _authService.RevokeRefreshTokenAsync(userId);
            _logger.LogInformation("User {UserId} logged out", userId);
        }
        return NoContent();
    }
}

