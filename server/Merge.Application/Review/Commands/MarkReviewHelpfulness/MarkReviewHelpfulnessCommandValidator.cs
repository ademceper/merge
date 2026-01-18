using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Review.Commands.MarkReviewHelpfulness;

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
