
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectManagement.Models;

public class Project
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Project name is required")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Project name must be between 3 and 200 characters")]
    public required string Name { get; set; }

    [Required(ErrorMessage = "Project description is required")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 1000 characters")]
    public required string Description { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Completion percentage: 0 = Not Started, 25 = In Progress,
    /// 50 = Testing, 75 = Review, 100 = Completed
    /// </summary>
    [Range(0, 100, ErrorMessage = "Status must be between 0 and 100")]
    public int Status { get; set; } = 0;

    [ForeignKey("User")]
    public int UserId { get; set; }

    public User? User { get; set; }
}
