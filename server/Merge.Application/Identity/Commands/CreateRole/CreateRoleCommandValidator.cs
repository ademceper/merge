using FluentValidation;
using Merge.Domain.Enums;

namespace Merge.Application.Identity.Commands.CreateRole;

public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Role name is required")
            .MaximumLength(256).WithMessage("Role name cannot exceed 256 characters")
            .MinimumLength(2).WithMessage("Role name must be at least 2 characters");

        RuleFor(x => x.RoleType)
            .IsInEnum().WithMessage("Invalid role type");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.PermissionIds)
            .NotNull().WithMessage("PermissionIds cannot be null")
            .When(x => x.PermissionIds is not null);

        RuleForEach(x => x.PermissionIds)
            .NotEmpty().WithMessage("Permission ID cannot be empty")
            .When(x => x.PermissionIds is not null && x.PermissionIds.Count > 0);
    }
}
