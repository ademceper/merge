using FluentValidation;

namespace Merge.Application.Review.Commands.MarkReviewHelpfulness;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class MarkReviewHelpfulnessCommandValidator : AbstractValidator<MarkReviewHelpfulnessCommand>
{
    public MarkReviewHelpfulnessCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("Kullanıcı ID'si zorunludur.");

        RuleFor(x => x.ReviewId)
            .NotEmpty()
            .WithMessage("Değerlendirme ID'si zorunludur.");
    }
}
