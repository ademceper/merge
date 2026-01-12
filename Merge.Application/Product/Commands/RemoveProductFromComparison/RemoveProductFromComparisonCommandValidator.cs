using FluentValidation;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.Product.Commands.RemoveProductFromComparison;

// ✅ BOLUM 2.1: Pipeline Behaviors - FluentValidation validators (ZORUNLU)
public class RemoveProductFromComparisonCommandValidator : AbstractValidator<RemoveProductFromComparisonCommand>
{
    public RemoveProductFromComparisonCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required");
    }
}
