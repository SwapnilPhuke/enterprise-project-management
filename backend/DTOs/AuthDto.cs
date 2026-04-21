using System.ComponentModel.DataAnnotations;

namespace ProjectManagement.DTOs;

/// <summary>Register a new user.</summary>
public class RegisterRequest
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 100 characters")]
    public required string Username { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [StringLength(255, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
    public required string Password { get; set; }

    [StringLength(255)]
    public string? FullName { get; set; }
}

/// <summary>Authenticate an existing user.</summary>
public class LoginRequest
{
    [Required(ErrorMessage = "Username is required")]
    public required string Username { get; set; }

    [Required(ErrorMessage = "Password is required")]
    public required string Password { get; set; }
}

/// <summary>Exchange a refresh token for a new access token.</summary>
public class RefreshRequest
{
    [Required]
    public required string RefreshToken { get; set; }
}
