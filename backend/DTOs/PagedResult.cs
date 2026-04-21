namespace ProjectManagement.DTOs;

/// <summary>Generic paginated result envelope.</summary>
public class PagedResult<T>
{
    public IEnumerable<T> Items     { get; init; } = [];
    public int TotalCount           { get; init; }
    public int Page                 { get; init; }
    public int PageSize             { get; init; }
    public int TotalPages           => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
    public bool HasPreviousPage     => Page > 1;
    public bool HasNextPage         => Page < TotalPages;
}
