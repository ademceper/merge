using FluentValidation;

namespace Merge.Application.Review.Commands.ApproveReview;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class ApproveReviewCommandValidator : AbstractValidator<ApproveReviewCommand>
{
    public ApproveReviewCommandValidator()
    {
        RuleFor(x => x.ReviewId)
            .NotEmpty()
            .WithMessage("Değerlendirme ID'si zorunludur.");

        RuleFor(x => x.ApprovedByUserId)
            .NotEmpty()
            .WithMessage("Onaylayan kullanıcı ID'si zorunludur.");
    }
}
