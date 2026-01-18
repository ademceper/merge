using FluentValidation;

namespace Merge.Application.Seller.Commands.ApproveSellerApplication;

public class ApproveSellerApplicationCommandValidator : AbstractValidator<ApproveSellerApplicationCommand>
{
    public ApproveSellerApplicationCommandValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty().WithMessage("Application ID is required.");

        RuleFor(x => x.ReviewerId)
            .NotEmpty().WithMessage("Reviewer ID is required.");
    }
}
