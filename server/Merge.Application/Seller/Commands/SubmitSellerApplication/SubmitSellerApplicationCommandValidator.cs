using FluentValidation;
using Merge.Domain.Enums;
using Merge.Domain.Modules.Identity;
using Merge.Domain.ValueObjects;

namespace Merge.Application.Seller.Commands.SubmitSellerApplication;

public class SubmitSellerApplicationCommandValidator : AbstractValidator<SubmitSellerApplicationCommand>
{
    public SubmitSellerApplicationCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.ApplicationDto)
            .NotNull().WithMessage("Application data is required.");

        When(x => x.ApplicationDto is not null, () =>
        {
            RuleFor(x => x.ApplicationDto!.BusinessName)
                .NotEmpty().WithMessage("Business name is required.")
                .MaximumLength(200).WithMessage("Business name must not exceed 200 characters.");

            RuleFor(x => x.ApplicationDto!.BusinessType)
                .IsInEnum().WithMessage("Business type must be a valid enum value.");

            RuleFor(x => x.ApplicationDto!.TaxNumber)
                .NotEmpty().WithMessage("Tax number is required.")
                .MaximumLength(50).WithMessage("Tax number must not exceed 50 characters.");

            RuleFor(x => x.ApplicationDto!.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.ApplicationDto!.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required.")
                .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters.");
        });
    }
}
