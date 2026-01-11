using FluentValidation;

namespace Merge.Application.Review.Commands.EvaluateProductBadges;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class EvaluateProductBadgesCommandValidator : AbstractValidator<EvaluateProductBadgesCommand>
{
    public EvaluateProductBadgesCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Ürün ID'si zorunludur.");
    }
}
