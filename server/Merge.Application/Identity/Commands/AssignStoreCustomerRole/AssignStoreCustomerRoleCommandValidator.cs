using FluentValidation;

namespace Merge.Application.Identity.Commands.AssignStoreCustomerRole;

public class AssignStoreCustomerRoleCommandValidator : AbstractValidator<AssignStoreCustomerRoleCommand>
{
    public AssignStoreCustomerRoleCommandValidator()
    {
        RuleFor(x => x.StoreId)
            .NotEmpty().WithMessage("Store ID is required");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("Role ID is required");
    }
}
