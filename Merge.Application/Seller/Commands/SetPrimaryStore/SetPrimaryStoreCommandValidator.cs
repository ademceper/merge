using FluentValidation;

namespace Merge.Application.Seller.Commands.SetPrimaryStore;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
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
