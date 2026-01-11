using FluentValidation;

namespace Merge.Application.Review.Commands.AwardSellerBadge;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class AwardSellerBadgeCommandValidator : AbstractValidator<AwardSellerBadgeCommand>
{
    public AwardSellerBadgeCommandValidator()
    {
        RuleFor(x => x.SellerId)
            .NotEmpty()
            .WithMessage("Satıcı ID'si zorunludur.");

        RuleFor(x => x.BadgeId)
            .NotEmpty()
            .WithMessage("Badge ID'si zorunludur.");

        RuleFor(x => x.AwardReason)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.AwardReason))
            .WithMessage("Ödül nedeni en fazla 500 karakter olabilir.");
    }
}
