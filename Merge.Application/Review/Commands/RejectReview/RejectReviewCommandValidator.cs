using FluentValidation;

namespace Merge.Application.Review.Commands.RejectReview;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class RejectReviewCommandValidator : AbstractValidator<RejectReviewCommand>
{
    public RejectReviewCommandValidator()
    {
        RuleFor(x => x.ReviewId)
            .NotEmpty()
            .WithMessage("Değerlendirme ID'si zorunludur.");

        RuleFor(x => x.RejectedByUserId)
            .NotEmpty()
            .WithMessage("Reddeden kullanıcı ID'si zorunludur.");

        RuleFor(x => x.Reason)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.Reason))
            .WithMessage("Red nedeni en fazla 1000 karakter olabilir.");
    }
}
