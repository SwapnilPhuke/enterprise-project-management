using System.ComponentModel.DataAnnotations;

namespace ProjectManagement.Models;

/// <summary>
/// User entity for authentication and authorization
/// </summary>
public class User
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Username is required")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 100 characters")]
    public required string Username { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public required string Email { get; set; }

    [Required]
    [StringLength(255)]  // BCrypt hashes are ~60 chars; 255 allows future algorithm changes
    public required string PasswordHash { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastLogin { get; set; }

    public bool IsActive { get; set; } = true;

    [StringLength(255)]
    public string? FullName { get; set; }

    /// <summary>Role: "User" (default) or "Admin"</summary>
    [Required]
    [StringLength(50)]
    public string Role { get; set; } = "User";

    // Navigation properties
    public ICollection<Project> Projects { get; set; } = new List<Project>();
}
