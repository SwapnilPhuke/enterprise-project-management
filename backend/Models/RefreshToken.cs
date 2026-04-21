using System.ComponentModel.DataAnnotations;

namespace ProjectManagement.Models;

/// <summary>
/// Persisted refresh token — allows issuing new access tokens without re-login.
/// </summary>
public class RefreshToken
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(256)]
    public required string Token { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive  => !IsRevoked && !IsExpired;
}
