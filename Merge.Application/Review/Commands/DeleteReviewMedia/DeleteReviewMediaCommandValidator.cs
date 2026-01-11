using FluentValidation;

namespace Merge.Application.Review.Commands.DeleteReviewMedia;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class DeleteReviewMediaCommandValidator : AbstractValidator<DeleteReviewMediaCommand>
{
    public DeleteReviewMediaCommandValidator()
    {
        RuleFor(x => x.MediaId)
            .NotEmpty()
            .WithMessage("Medya ID'si zorunludur.");
    }
}
