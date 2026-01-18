using FluentValidation;
using Merge.Domain.Modules.Marketplace;

namespace Merge.Application.Seller.Commands.VerifyStore;

public class VerifyStoreCommandValidator : AbstractValidator<VerifyStoreCommand>
{
    public VerifyStoreCommandValidator()
    {
        RuleFor(x => x.StoreId)
            .NotEmpty().WithMessage("Store ID is required.");
    }
}
