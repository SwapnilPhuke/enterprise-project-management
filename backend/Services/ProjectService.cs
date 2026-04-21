using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using ProjectManagement.Common;
using ProjectManagement.DTOs;
using ProjectManagement.Models;
using ProjectManagement.Repositories;

namespace ProjectManagement.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _repository;
    private readonly IMapper            _mapper;
    private readonly IMemoryCache       _cache;

    public ProjectService(IProjectRepository repository, IMapper mapper, IMemoryCache cache)
    {
        _repository = repository;
        _mapper     = mapper;
        _cache      = cache;
    }

    public async Task<ServiceResult<PagedResult<ProjectResponseDto>>> GetPagedAsync(int userId, ProjectQueryParams query)
    {
        var (items, totalCount) = await _repository.GetPagedAsync(userId, query);

        var result = new PagedResult<ProjectResponseDto>
        {
            Items      = _mapper.Map<IEnumerable<ProjectResponseDto>>(items),
            TotalCount = totalCount,
            Page       = query.Page,
            PageSize   = query.PageSize
        };

        return ServiceResult<PagedResult<ProjectResponseDto>>.Ok(result);
    }

    public async Task<ServiceResult<ProjectResponseDto>> GetByIdAsync(int id, int userId)
    {
        var project = await _repository.GetByIdAsync(id, userId);

        return project is null
            ? ServiceResult<ProjectResponseDto>.NotFound($"Project {id} not found")
            : ServiceResult<ProjectResponseDto>.Ok(_mapper.Map<ProjectResponseDto>(project));
    }

    public async Task<ServiceResult<ProjectStatsDto>> GetStatsAsync(int userId)
    {
        var cacheKey = $"stats_{userId}";
        if (_cache.TryGetValue(cacheKey, out ProjectStatsDto? cached))
            return ServiceResult<ProjectStatsDto>.Ok(cached!);

        var statuses = await _repository.GetStatusListAsync(userId);

        var stats = new ProjectStatsDto(
            Total:          statuses.Count,
            NotStarted:     statuses.Count(s => s == 0),
            InProgress:     statuses.Count(s => s == 25),
            Testing:        statuses.Count(s => s == 50),
            Review:         statuses.Count(s => s == 75),
            Completed:      statuses.Count(s => s == 100),
            CompletionRate: statuses.Count > 0
                ? Math.Round((double)statuses.Count(s => s == 100) / statuses.Count * 100, 1)
                : 0
        );

        _cache.Set(cacheKey, stats, TimeSpan.FromMinutes(5));
        return ServiceResult<ProjectStatsDto>.Ok(stats);
    }

    public async Task<ServiceResult<ProjectResponseDto>> CreateAsync(ProjectDto dto, int userId)
    {
        var project = new Project
        {
            Name        = dto.Name,
            Description = dto.Description,
            Status      = dto.Status,
            UserId      = userId
        };

        var created = await _repository.AddAsync(project);
        InvalidateStats(userId);
        return ServiceResult<ProjectResponseDto>.Ok(_mapper.Map<ProjectResponseDto>(created));
    }

    public async Task<ServiceResult<ProjectResponseDto>> UpdateAsync(int id, ProjectUpdateDto dto, int userId)
    {
        var project = await _repository.GetByIdAsync(id, userId);
        if (project is null)
            return ServiceResult<ProjectResponseDto>.NotFound($"Project {id} not found");

        project.Name        = dto.Name        ?? project.Name;
        project.Description = dto.Description ?? project.Description;
        project.Status      = dto.Status      ?? project.Status;

        await _repository.UpdateAsync(project);
        InvalidateStats(userId);
        return ServiceResult<ProjectResponseDto>.Ok(_mapper.Map<ProjectResponseDto>(project));
    }

    public async Task<ServiceResult> DeleteAsync(int id, int userId)
    {
        var deleted = await _repository.DeleteAsync(id, userId);
        if (!deleted)
            return ServiceResult.NotFound($"Project {id} not found");

        InvalidateStats(userId);
        return ServiceResult.Ok();
    }

    private void InvalidateStats(int userId) => _cache.Remove($"stats_{userId}");
}

