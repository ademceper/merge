using FluentValidation;

namespace Merge.Application.Review.Commands.RevokeProductBadge;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class RevokeProductBadgeCommandValidator : AbstractValidator<RevokeProductBadgeCommand>
{
    public RevokeProductBadgeCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Ürün ID'si zorunludur.");

        RuleFor(x => x.BadgeId)
            .NotEmpty()
            .WithMessage("Badge ID'si zorunludur.");
    }
}
