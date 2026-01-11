using FluentValidation;
using Merge.Domain.Enums;

namespace Merge.Application.Seller.Commands.ReviewSellerApplication;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class ReviewSellerApplicationCommandValidator : AbstractValidator<ReviewSellerApplicationCommand>
{
    public ReviewSellerApplicationCommandValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty().WithMessage("Application ID is required.");

        RuleFor(x => x.ReviewerId)
            .NotEmpty().WithMessage("Reviewer ID is required.");

        When(x => x.Status == SellerApplicationStatus.Rejected, () =>
        {
            RuleFor(x => x.RejectionReason)
                .NotEmpty().WithMessage("Rejection reason is required when rejecting an application.")
                .MaximumLength(1000).WithMessage("Rejection reason must not exceed 1000 characters.");
        });
    }
}
