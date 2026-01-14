using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Review.Commands.DeleteReview;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class DeleteReviewCommandValidator : AbstractValidator<DeleteReviewCommand>
{
    public DeleteReviewCommandValidator()
    {
        RuleFor(x => x.ReviewId)
            .NotEmpty()
            .WithMessage("Değerlendirme ID'si zorunludur.");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("Kullanıcı ID'si zorunludur.");
    }
}
