using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Review.Commands.EvaluateSellerBadges;

public class EvaluateSellerBadgesCommandValidator : AbstractValidator<EvaluateSellerBadgesCommand>
{
    public EvaluateSellerBadgesCommandValidator()
    {
        RuleFor(x => x.SellerId)
            .NotEmpty()
            .WithMessage("Satıcı ID'si zorunludur.");
    }
}
