using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Review.Commands.RevokeSellerBadge;

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
