using FluentValidation;

namespace Merge.Application.Identity.Commands.AssignOrganizationRole;

public class AssignOrganizationRoleCommandValidator : AbstractValidator<AssignOrganizationRoleCommand>
{
    public AssignOrganizationRoleCommandValidator()
    {
        RuleFor(x => x.OrganizationId)
            .NotEmpty().WithMessage("Organization ID is required");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("Role ID is required");
    }
}
