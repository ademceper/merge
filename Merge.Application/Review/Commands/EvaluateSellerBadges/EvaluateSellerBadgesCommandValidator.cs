using FluentValidation;

namespace Merge.Application.Review.Commands.EvaluateSellerBadges;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class EvaluateSellerBadgesCommandValidator : AbstractValidator<EvaluateSellerBadgesCommand>
{
    public EvaluateSellerBadgesCommandValidator()
    {
        RuleFor(x => x.SellerId)
            .NotEmpty()
            .WithMessage("Satıcı ID'si zorunludur.");
    }
}
