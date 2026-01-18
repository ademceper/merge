using FluentValidation;
using Merge.Domain.Modules.Marketplace;

namespace Merge.Application.Seller.Commands.CreateStore;

public class CreateStoreCommandValidator : AbstractValidator<CreateStoreCommand>
{
    public CreateStoreCommandValidator()
    {
        RuleFor(x => x.SellerId)
            .NotEmpty().WithMessage("Seller ID is required.");

        RuleFor(x => x.Dto)
            .NotNull().WithMessage("Store data is required.");

        When(x => x.Dto != null, () =>
        {
            RuleFor(x => x.Dto!.StoreName)
                .NotEmpty().WithMessage("Store name is required.")
                .MaximumLength(200).WithMessage("Store name must not exceed 200 characters.");

            RuleFor(x => x.Dto!.ContactEmail)
                .EmailAddress().When(x => !string.IsNullOrEmpty(x.Dto!.ContactEmail))
                .WithMessage("Invalid email format.");

            RuleFor(x => x.Dto!.ContactPhone)
                .MaximumLength(20).When(x => !string.IsNullOrEmpty(x.Dto!.ContactPhone))
                .WithMessage("Phone number must not exceed 20 characters.");
        });
    }
}
