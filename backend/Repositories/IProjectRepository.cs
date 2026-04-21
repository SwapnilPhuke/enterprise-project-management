using ProjectManagement.DTOs;
using ProjectManagement.Models;

namespace ProjectManagement.Repositories;

public interface IProjectRepository
{
    Task<(IEnumerable<Project> Items, int TotalCount)> GetPagedAsync(int userId, ProjectQueryParams query);
    Task<Project?> GetByIdAsync(int id, int userId);
    Task<Project> AddAsync(Project project);
    Task UpdateAsync(Project project);
    Task<bool> DeleteAsync(int id, int userId);
    Task<List<int>> GetStatusListAsync(int userId);
}
