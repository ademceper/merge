using FluentValidation;

namespace Merge.Application.Seller.Commands.RejectSellerApplication;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class RejectSellerApplicationCommandValidator : AbstractValidator<RejectSellerApplicationCommand>
{
    public RejectSellerApplicationCommandValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty().WithMessage("Application ID is required.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Rejection reason is required.")
            .MaximumLength(1000).WithMessage("Rejection reason must not exceed 1000 characters.");

        RuleFor(x => x.ReviewerId)
            .NotEmpty().WithMessage("Reviewer ID is required.");
    }
}
