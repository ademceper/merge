using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Review.Commands.AwardProductBadge;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class AwardProductBadgeCommandValidator : AbstractValidator<AwardProductBadgeCommand>
{
    public AwardProductBadgeCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Ürün ID'si zorunludur.");

        RuleFor(x => x.BadgeId)
            .NotEmpty()
            .WithMessage("Badge ID'si zorunludur.");

        RuleFor(x => x.AwardReason)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.AwardReason))
            .WithMessage("Ödül nedeni en fazla 500 karakter olabilir.");
    }
}
