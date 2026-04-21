using System.ComponentModel.DataAnnotations;

namespace ProjectManagement.DTOs;

public class ProjectDto
{
    [Required(ErrorMessage = "Project name is required")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Project name must be between 3 and 200 characters")]
    public required string Name { get; set; }

    [Required(ErrorMessage = "Project description is required")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 1000 characters")]
    public required string Description { get; set; }

    [Range(0, 100, ErrorMessage = "Status must be between 0 and 100")]
    public int Status { get; set; } = 0;
}

public class ProjectUpdateDto
{
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Project name must be between 3 and 200 characters")]
    public string? Name { get; set; }

    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 1000 characters")]
    public string? Description { get; set; }

    [Range(0, 100, ErrorMessage = "Status must be between 0 and 100")]
    public int? Status { get; set; }
}
