using FluentValidation;
using Merge.Domain.Modules.Marketplace;

namespace Merge.Application.Seller.Commands.SuspendStore;

public class SuspendStoreCommandValidator : AbstractValidator<SuspendStoreCommand>
{
    public SuspendStoreCommandValidator()
    {
        RuleFor(x => x.StoreId)
            .NotEmpty().WithMessage("Store ID is required.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Suspension reason is required.")
            .MaximumLength(500).WithMessage("Reason must not exceed 500 characters.");
    }
}
