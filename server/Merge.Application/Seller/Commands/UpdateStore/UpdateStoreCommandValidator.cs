using FluentValidation;
using Merge.Domain.Modules.Marketplace;

namespace Merge.Application.Seller.Commands.UpdateStore;

public class UpdateStoreCommandValidator : AbstractValidator<UpdateStoreCommand>
{
    public UpdateStoreCommandValidator()
    {
        RuleFor(x => x.StoreId)
            .NotEmpty().WithMessage("Store ID is required.");

        RuleFor(x => x.Dto)
            .NotNull().WithMessage("Store data is required.");

        When(x => x.Dto is not null, () =>
        {
            RuleFor(x => x.Dto!.StoreName)
                .MaximumLength(200).When(x => !string.IsNullOrEmpty(x.Dto!.StoreName))
                .WithMessage("Store name must not exceed 200 characters.");

            RuleFor(x => x.Dto!.ContactEmail)
                .EmailAddress().When(x => !string.IsNullOrEmpty(x.Dto!.ContactEmail))
                .WithMessage("Invalid email format.");

            RuleFor(x => x.Dto!.ContactPhone)
                .MaximumLength(20).When(x => !string.IsNullOrEmpty(x.Dto!.ContactPhone))
                .WithMessage("Phone number must not exceed 20 characters.");
        });
    }
}
