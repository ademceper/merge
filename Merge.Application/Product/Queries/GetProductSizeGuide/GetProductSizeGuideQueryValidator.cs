using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Queries.GetProductSizeGuide;

// ✅ BOLUM 2.1: Pipeline Behaviors - FluentValidation validators (ZORUNLU)
public class GetProductSizeGuideQueryValidator : AbstractValidator<GetProductSizeGuideQuery>
{
    public GetProductSizeGuideQueryValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required");
    }
}
