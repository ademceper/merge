using FluentValidation;

namespace Merge.Application.Seller.Commands.VerifyStore;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class VerifyStoreCommandValidator : AbstractValidator<VerifyStoreCommand>
{
    public VerifyStoreCommandValidator()
    {
        RuleFor(x => x.StoreId)
            .NotEmpty().WithMessage("Store ID is required.");
    }
}
