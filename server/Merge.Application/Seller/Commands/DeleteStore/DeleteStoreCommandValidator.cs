using FluentValidation;
using Merge.Domain.Modules.Marketplace;

namespace Merge.Application.Seller.Commands.DeleteStore;

public class DeleteStoreCommandValidator : AbstractValidator<DeleteStoreCommand>
{
    public DeleteStoreCommandValidator()
    {
        RuleFor(x => x.StoreId)
            .NotEmpty().WithMessage("Store ID is required.");
    }
}
