using FluentValidation;
using Merge.Domain.Modules.Marketplace;

namespace Merge.Application.Seller.Commands.SetPrimaryStore;

public class SetPrimaryStoreCommandValidator : AbstractValidator<SetPrimaryStoreCommand>
{
    public SetPrimaryStoreCommandValidator()
    {
        RuleFor(x => x.SellerId)
            .NotEmpty().WithMessage("Seller ID is required.");

        RuleFor(x => x.StoreId)
            .NotEmpty().WithMessage("Store ID is required.");
    }
}
