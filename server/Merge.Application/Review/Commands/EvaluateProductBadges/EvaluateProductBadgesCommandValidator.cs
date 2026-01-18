using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Review.Commands.EvaluateProductBadges;

public class EvaluateProductBadgesCommandValidator : AbstractValidator<EvaluateProductBadgesCommand>
{
    public EvaluateProductBadgesCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Ürün ID'si zorunludur.");
    }
}
