using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Review.Commands.DeleteReviewMedia;

public class DeleteReviewMediaCommandValidator : AbstractValidator<DeleteReviewMediaCommand>
{
    public DeleteReviewMediaCommandValidator()
    {
        RuleFor(x => x.MediaId)
            .NotEmpty()
            .WithMessage("Medya ID'si zorunludur.");
    }
}
