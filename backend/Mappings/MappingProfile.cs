using AutoMapper;
using ProjectManagement.DTOs;
using ProjectManagement.Models;

namespace ProjectManagement.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Project, ProjectResponseDto>()
            .ForMember(dst => dst.StatusLabel,
                       opt => opt.MapFrom(src => GetStatusLabel(src.Status)));
    }

    private static string GetStatusLabel(int status) => status switch
    {
        0   => "Not Started",
        25  => "In Progress",
        50  => "Testing",
        75  => "Review",
        100 => "Completed",
        _   => "Unknown"
    };
}
