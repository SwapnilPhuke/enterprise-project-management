namespace ProjectManagement.DTOs;

/// <summary>
/// Safe API response shape for a project.
/// Never exposes the User navigation entity — prevents data leakage and circular-reference issues.
/// </summary>
public class ProjectResponseDto
{
    public int    Id          { get; set; }
    public string Name        { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int    Status      { get; set; }
    public string StatusLabel { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int    UserId      { get; set; }
}
