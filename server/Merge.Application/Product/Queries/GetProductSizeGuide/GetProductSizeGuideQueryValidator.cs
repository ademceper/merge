using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Queries.GetProductSizeGuide;

public class GetProductSizeGuideQueryValidator : AbstractValidator<GetProductSizeGuideQuery>
{
    public GetProductSizeGuideQueryValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required");
    }
}
