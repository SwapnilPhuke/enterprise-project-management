using System.ComponentModel.DataAnnotations;

namespace ProjectManagement.DTOs;

/// <summary>Query parameters for paginated / filtered project lists.</summary>
public class ProjectQueryParams
{
    private int _pageSize = 10;

    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 50)]
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = Math.Clamp(value, 1, 50);
    }

    /// <summary>Searches Name and Description (case-insensitive).</summary>
    public string? Search { get; set; }

    /// <summary>Filter by exact status value (0/25/50/75/100).</summary>
    public int? Status { get; set; }

    /// <summary>Sort field: createdAt | updatedAt | name | status</summary>
    public string SortBy { get; set; } = "createdAt";

    /// <summary>true = descending (newest first).</summary>
    public bool SortDesc { get; set; } = true;
}
