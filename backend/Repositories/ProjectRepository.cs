using Microsoft.EntityFrameworkCore;
using ProjectManagement.Data;
using ProjectManagement.DTOs;
using ProjectManagement.Models;

namespace ProjectManagement.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly AppDbContext _context;

    public ProjectRepository(AppDbContext context) => _context = context;

    public async Task<(IEnumerable<Project> Items, int TotalCount)> GetPagedAsync(int userId, ProjectQueryParams q)
    {
        var query = _context.Projects.Where(p => p.UserId == userId);

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var term = q.Search.Trim().ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(term) ||
                p.Description.ToLower().Contains(term));
        }

        if (q.Status.HasValue)
            query = query.Where(p => p.Status == q.Status.Value);

        query = q.SortBy?.ToLower() switch
        {
            "name"      => q.SortDesc ? query.OrderByDescending(p => p.Name)      : query.OrderBy(p => p.Name),
            "status"    => q.SortDesc ? query.OrderByDescending(p => p.Status)    : query.OrderBy(p => p.Status),
            "updatedat" => q.SortDesc ? query.OrderByDescending(p => p.UpdatedAt) : query.OrderBy(p => p.UpdatedAt),
            _           => q.SortDesc ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
        };

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Project?> GetByIdAsync(int id, int userId) =>
        await _context.Projects.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

    public async Task<Project> AddAsync(Project project)
    {
        project.CreatedAt = project.UpdatedAt = DateTime.UtcNow;
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();
        return project;
    }

    public async Task UpdateAsync(Project project)
    {
        project.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(int id, int userId)
    {
        var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
        if (project is null) return false;

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<int>> GetStatusListAsync(int userId) =>
        await _context.Projects
            .Where(p => p.UserId == userId)
            .Select(p => p.Status)
            .ToListAsync();
}
