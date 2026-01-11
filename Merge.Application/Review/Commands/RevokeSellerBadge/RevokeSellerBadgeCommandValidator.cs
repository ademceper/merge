using FluentValidation;

namespace Merge.Application.Review.Commands.RevokeSellerBadge;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class RevokeSellerBadgeCommandValidator : AbstractValidator<RevokeSellerBadgeCommand>
{
    public RevokeSellerBadgeCommandValidator()
    {
        RuleFor(x => x.SellerId)
            .NotEmpty()
            .WithMessage("Satıcı ID'si zorunludur.");

        RuleFor(x => x.BadgeId)
            .NotEmpty()
            .WithMessage("Badge ID'si zorunludur.");
    }
}
