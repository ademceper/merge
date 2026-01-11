using FluentValidation;

namespace Merge.Application.Seller.Commands.DeleteStore;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class DeleteStoreCommandValidator : AbstractValidator<DeleteStoreCommand>
{
    public DeleteStoreCommandValidator()
    {
        RuleFor(x => x.StoreId)
            .NotEmpty().WithMessage("Store ID is required.");
    }
}
