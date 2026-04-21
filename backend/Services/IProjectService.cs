
using ProjectManagement.Common;
using ProjectManagement.DTOs;

namespace ProjectManagement.Services;

public interface IProjectService
{
    Task<ServiceResult<PagedResult<ProjectResponseDto>>> GetPagedAsync(int userId, ProjectQueryParams query);
    Task<ServiceResult<ProjectResponseDto>>             GetByIdAsync(int id, int userId);
    Task<ServiceResult<ProjectStatsDto>>                GetStatsAsync(int userId);
    Task<ServiceResult<ProjectResponseDto>>             CreateAsync(ProjectDto dto, int userId);
    Task<ServiceResult<ProjectResponseDto>>             UpdateAsync(int id, ProjectUpdateDto dto, int userId);
    Task<ServiceResult>                                 DeleteAsync(int id, int userId);
}
