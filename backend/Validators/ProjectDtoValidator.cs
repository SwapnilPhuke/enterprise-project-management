using FluentValidation;
using ProjectManagement.DTOs;

namespace ProjectManagement.Validators;

public class ProjectDtoValidator : AbstractValidator<ProjectDto>
{
    private static readonly int[] ValidStatuses = [0, 25, 50, 75, 100];

    public ProjectDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Project name is required")
            .MinimumLength(3).WithMessage("Name must be at least 3 characters")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MinimumLength(10).WithMessage("Description must be at least 10 characters")
            .MaximumLength(1000).WithMessage("Description must not exceed 1,000 characters");

        RuleFor(x => x.Status)
            .Must(s => ValidStatuses.Contains(s))
            .WithMessage("Status must be 0 (Not Started), 25 (In Progress), 50 (Testing), 75 (Review), or 100 (Completed)");
    }
}
