using FluentValidation;

namespace Merge.Application.Product.Commands.DeleteSizeGuide;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class DeleteSizeGuideCommandValidator : AbstractValidator<DeleteSizeGuideCommand>
{
    public DeleteSizeGuideCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Beden kılavuzu ID boş olamaz.");
    }
}
