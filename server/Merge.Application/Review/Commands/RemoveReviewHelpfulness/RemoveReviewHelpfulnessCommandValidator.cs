using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Review.Commands.RemoveReviewHelpfulness;

public class RemoveReviewHelpfulnessCommandValidator : AbstractValidator<RemoveReviewHelpfulnessCommand>
{
    public RemoveReviewHelpfulnessCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("Kullanıcı ID'si zorunludur.");

        RuleFor(x => x.ReviewId)
            .NotEmpty()
            .WithMessage("Değerlendirme ID'si zorunludur.");
    }
}
